using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Data;

namespace SQLEntraID_4_6_2
{
    public partial class Form1 : Form
    {
        // Reuse credential objects to take advantage of underlying token caches.
        private static readonly ConcurrentDictionary<string, DefaultAzureCredential> credentials = new ConcurrentDictionary<string, DefaultAzureCredential>();
        private const string defaultScopeSuffix = "/.default";

        // Token cache helper for persistent caching
        private static class TokenCacheHelper
        {
            private static readonly string CacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "msal_cache.json");
            private static readonly object FileLock = new object();

            public static void EnablePersistence(ITokenCache tokenCache)
            {
                tokenCache.SetBeforeAccess(BeforeAccessNotification);
                tokenCache.SetAfterAccess(AfterAccessNotification);
            }

            private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
            {
                lock (FileLock)
                {
                    if (File.Exists(CacheFilePath))
                    {
                        args.TokenCache.DeserializeMsalV3(File.ReadAllBytes(CacheFilePath));
                    }
                }
            }

            private static void AfterAccessNotification(TokenCacheNotificationArgs args)
            {
                if (args.HasStateChanged)
                {
                    lock (FileLock)
                    {
                        File.WriteAllBytes(CacheFilePath, args.TokenCache.SerializeMsalV3());
                    }
                }
            }
        }

        private async Task<AuthenticationResult> LoginAsync()
        {
            var app = PublicClientApplicationBuilder.Create("65fd1e99-ac4a-4d12-8519-9dc0c48a1702")
                .WithAuthority(AzureCloudInstance.AzurePublic, "7da854e2-6115-4de1-bf5a-9e7af4fc3c98")
                .WithRedirectUri("http://localhost")
                .WithLogging((level, message, containsPii) => Debug.WriteLine($"MSAL: {message}"), LogLevel.Verbose, true, true)
                .Build();

            // Enable persistent token caching
            TokenCacheHelper.EnablePersistence(app.UserTokenCache);

            string[] scopes = new string[] { "https://database.windows.net//.default" };

            try
            {
                // Check for cached accounts
                var accounts = await app.GetAccountsAsync();
                if (accounts == null || !accounts.Any())
                {
                    MessageBox.Show("No cached accounts found. Please log in into EntraID F.", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return await app.AcquireTokenInteractive(scopes).ExecuteAsync();
                }

                // Display the account name being used
                var account = accounts.FirstOrDefault();
          
                if (account != null)
                {
                    MessageBox.Show($"Attempting silent authentication for account: {account.Username}", "Silent Authentication", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Try silent authentication
                return await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // Fallback to interactive authentication
                MessageBox.Show("Silent authentication failed. Please log in interactively.", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return await app.AcquireTokenInteractive(scopes).ExecuteAsync();
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string connectionString = @"Server=192.168.88.198;
Database=AdventureWorks2019;
Encrypt=True;
TrustServerCertificate=True;";

            try
            {
                // Acquire the user's Entra ID token
                var result = await LoginAsync();

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.AccessToken = result.AccessToken; // Set the token directly
                    conn.Open();
                    MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string domain = Environment.UserDomainName;
            string username = Environment.UserName;

            if (domain.Equals("azuread", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($"Logged in with an Entra ID user: {domain}\\{username}", "User Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Not logged in with an Entra ID user. Current user: {domain}\\{username}", "User Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task<DataTable> FetchDataAsync()
        {
            string connectionString = @"Server=192.168.88.198;
Database=AdventureWorks2019;
Encrypt=True;
TrustServerCertificate=True;";

            var dataTable = new DataTable();

            try
            {
                var result = await LoginAsync(); // Acquire token

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.AccessToken = result.AccessToken; // Set the token
                    await conn.OpenAsync();

                    using (var cmd = new SqlCommand("SELECT TOP (1000) [FirstName], [MiddleName], [LastName] FROM [AdventureWorks2019].[Person].[Person]", conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dataTable;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var data = await FetchDataAsync();
            dataGridView1.DataSource = data;
        }
    }
}