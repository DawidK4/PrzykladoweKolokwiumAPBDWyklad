using System.Data;
using System.Data.SqlClient;
using Kolokwium1.DTOs;

namespace Kolokwium1.Services;

public interface IDbService
{
    public Task<List<Perscription>?> GetPerscriptions(string docName);
    public Task<int> GetDocId(string docName);
    public Task<Perscription?> AddPerscription(Perscription perscription);
}

public class DbService : IDbService
{
    private readonly string _connectionString = @"Data Source=DAWID\SQLEXPRESS;Initial Catalog=master;Integrated Security=True";

    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(_connectionString);
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }

    public async Task<int> GetDocId(string docName)
    {
        await using var connection = await GetConnection();

        var getDocCommand = new SqlCommand();
        getDocCommand.Connection = connection;
        getDocCommand.CommandText = @"
                                    select d.IdDoctor
                                    from Doctor d
                                    where d.FirstName = @FirstName
                                    ";
        getDocCommand.Parameters.AddWithValue("@FirstName", docName);
        var reader = await getDocCommand.ExecuteReaderAsync();

        if (!reader.HasRows) return -1;
        await reader.ReadAsync();

        int docId = reader.GetInt32(0);
        return docId;
    }

    public async Task<List<Perscription>> GetPerscriptions(string docName)
    {
        await using var connection = await GetConnection();
        var getPerscriptionCommand = new SqlCommand();
        getPerscriptionCommand.Connection = connection;

        string commandText = null;
        int docId = await GetDocId(docName);

        if (docName == null)
        {
            commandText = @"
                        SELECT *
                        FROM Prescription p
                        ORDER BY p.Date DESC;
                        ";
            getPerscriptionCommand.CommandText = commandText;
        }
        else
        {
            commandText = @"
                        SELECT * 
                        FROM Prescription p
                        WHERE p.IdDoctor = @IdDoctor
                        ORDER BY p.Date DESC
                        ";
            getPerscriptionCommand.CommandText = commandText;
            getPerscriptionCommand.Parameters.AddWithValue("@IdDoctor", docId); // Pass docId directly
        }

        var reader = await getPerscriptionCommand.ExecuteReaderAsync();
        if (!reader.HasRows) return null;

        List<Perscription> perscriptions = new List<Perscription>();

        while (await reader.ReadAsync())
        {
            Perscription perscription = new Perscription
            {
                IdPerscription = reader.GetInt32(0),
                Date = reader.GetDateTime(1),
                DueDate = reader.GetDateTime(2),
                IdPatient = reader.GetInt32(3),
                IdDoctor = reader.GetInt32(4)
            };
            perscriptions.Add(perscription);
        }

        return perscriptions;
    }

    public async Task<Perscription?> AddPerscription(Perscription perscription)
    {
        try
        {
            if (perscription.Date >= perscription.DueDate)
            {
                throw new Exception("Due date must be later than prescription date.");
            }

            await using var connection = await GetConnection();

            var addNewPrescription = new SqlCommand();
            addNewPrescription.Connection = connection;
            addNewPrescription.CommandText =
                @"INSERT INTO Prescription (Date, DueDate, IdPatient, IdDoctor) VALUES (@Date, @DueDate, @IdPatient, @IdDoctor); SELECT SCOPE_IDENTITY();";

            addNewPrescription.Parameters.AddWithValue("@Date", perscription.Date);
            addNewPrescription.Parameters.AddWithValue("@DueDate", perscription.DueDate);
            addNewPrescription.Parameters.AddWithValue("@IdPatient", perscription.IdPatient);
            addNewPrescription.Parameters.AddWithValue("@IdDoctor", perscription.IdDoctor);

            var id = await addNewPrescription.ExecuteScalarAsync();
            perscription.IdPerscription = Convert.ToInt32(id);

            return perscription;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding prescription: {ex.Message}");
            return null;
        }
    }
}