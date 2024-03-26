/////////////////////Pinger///////////////////
using Npgsql;
using System;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

// Переменные окружения
string intervalStr = Environment.GetEnvironmentVariable("INTERVAL");
string logFilePath = Environment.GetEnvironmentVariable("LOG_FILE_PATH");

string GetSecretWithAppRole()
{
    var vaultAddr = Environment.GetEnvironmentVariable("VAULT_ADDR");
    if (String.IsNullOrEmpty(vaultAddr))
    {
        throw new System.ArgumentNullException("Vault Address");
    }

    var roleId = Environment.GetEnvironmentVariable("APPROLE_ROLE_ID");
    if (String.IsNullOrEmpty(roleId))
    {
        throw new System.ArgumentNullException("AppRole Role Id");
    }

    var secretPath = Environment.GetEnvironmentVariable("SECRET_PATH");
    if (String.IsNullOrEmpty(secretPath))
    {
        throw new System.ArgumentNullException("Secret path");
    }

    var secretMount = Environment.GetEnvironmentVariable("SECRET_MOUNT");
    if (String.IsNullOrEmpty(secretMount))
    {
        throw new System.ArgumentNullException("Secret mount");
    }

    var wrappingToken = Environment.GetEnvironmentVariable("WRAPPING_TOKEN");
    if (String.IsNullOrEmpty(wrappingToken))
    {
        throw new System.ArgumentNullException("Wrapping token");
    }

    // Необходимо создать два VaultClient объекта для аутентификации с помощью AppRole. 
    // Сперва необходимо использовать unwrap utility.
    // Необходимо инициализировать клиента с помощью wrapping token.
    IAuthMethodInfo wrappedTokenAuthMethod = new TokenAuthMethodInfo(wrappingToken);
    var vaultClientSettingsForUnwrapping = new VaultClientSettings(vaultAddr, wrappedTokenAuthMethod);

    IVaultClient vaultClientForUnwrapping = new VaultClient(vaultClientSettingsForUnwrapping);

    // Здесь передается null вместо wrapping token, чтобы не исчерпать его однократное использование
    // Учитывая, что клиент инициализирован уже с помощью токена-обертки
    Secret<Dictionary<string, object>> secretIdData = vaultClientForUnwrapping.V1.System
        .UnwrapWrappedResponseDataAsync<Dictionary<string, object>>(null).Result;

    var secretId = secretIdData.Data["secret_id"]; // Получаем secret_id

    // Создаем второй VaultClient и инициализируем его с AppRole auth method и с полученными credentials.
    IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(roleId, secretId.ToString());
    var vaultClientSettings = new VaultClientSettings(vaultAddr, authMethod);

    IVaultClient vaultClient = new VaultClient(vaultClientSettings);

    // Получаем секрет из VaultClient
    Secret<Dictionary<string, object>> secret = null;
    secret = vaultClient.V1.Secrets.KeyValue.V1.ReadSecretAsync(path: secretPath, mountPoint: secretMount).Result;

    var connection = secret.Data["Connection"];
    return connection.ToString();
}


var connectionString = GetSecretWithAppRole();

// Парсинг значения интервала (в секундах)
double interval;
if (!double.TryParse(intervalStr, out interval))
{
    interval = 300; // Значение по умолчанию, если интервал не удалось распарсить
}

var tasklist = new List<Task>();

while (true)
{

    lock (tasklist)
    {
        tasklist.RemoveAll(x => x.IsCompleted || x.IsFaulted);

        if (tasklist.Count > 0)
        {
            using (StreamWriter logWriter = new StreamWriter(logFilePath, true))
            {
                // Сообщение записывается в файл
                logWriter.WriteLine($"{DateTime.Now}:Task is paused");
            }
            // Выводит сообщение в stdout
            Console.WriteLine($"{DateTime.Now}:Task is paused");
        }
        tasklist.Clear();

        var task = Task.Run(() =>
            {
                DoHeartbeat(connectionString);
            });

        tasklist.Add(task);

    }

    await Task.Delay(TimeSpan.FromSeconds(interval));
}

void DoHeartbeat(object state)
{
    string connectionString = (string)state;

    try
    {
        // Открытие подключения к БД
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            // Команда полкчение версии БД
            using (NpgsqlCommand command = new NpgsqlCommand("SELECT version()", connection))
            {
                // Результат записывается в переменную
                string version = (string)command.ExecuteScalar();

                using (StreamWriter logWriter = new StreamWriter(logFilePath, true))
                {
                    // Сообщение записывается в файл
                    logWriter.WriteLine($"{DateTime.Now}:PostgreSQL version: " + version);
                }
                // Выводит сообщение в stdout
                Console.WriteLine($"{DateTime.Now}:PostgreSQL version: " + version);
            }
        }
    }
    catch (Exception ex)
    {
        using (StreamWriter logWriter = new StreamWriter(logFilePath, true))
        {
            // Сообщение записывается в файл
            logWriter.WriteLine($"{DateTime.Now}:Error: " + ex.Message);
        }
        // Выводит ошибку в stderr
        Console.Error.WriteLine($"{DateTime.Now}:Error: " + ex.Message);
    }

}