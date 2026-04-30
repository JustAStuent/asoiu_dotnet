using Microsoft.Data.Sqlite;
using System;

/// <summary>
/// Система управления базами данных и таблицами
/// <\summary>
class DatabaseManager
{
	private string _path;

	public DatabaseManager(string path)
	{
		if (!File.Exists(path)) {
			File.Create(path);
		}

		_path = path;

		using var connection = new SqliteConnection($"Data Source={path}");

		connection.Open();

		var command = connection.CreateCommand();

		command.CommandText = @"
			CREATE TABLE IF NOT EXISTS district ( 
				district_id    INTEGER PRIMARY KEY AUTOINCREMENT,
				district_name  TEXT NOT NULL
			)";
		command.ExecuteNonQuery();
		
		command.CommandText = @"
			CREATE TABLE IF NOT EXISTS building (
				building_id    INTEGER PRIMARY KEY AUTOINCREMENT,
				district_id    INTEGER NOT NULL,
				building_name  TEXT NOT NULL,
				floors	       INTEGER NOT NULL
			)";
		command.ExecuteNonQuery();
	}


	public void ImportBuildingCsv(string path)
	{
		using var connection = new SqliteConnection($"Data Source={_path}");

		connection.Open();

		string[] lines = File.ReadAllLines(path);

		for (int i = 1; i < lines.Length; i++)
		{
			string[] parts = lines[i].Split(';');
			if (parts.Length < 2) continue;

			var command = connection.CreateCommand();
			command.CommandText = @"
				INSERT INTO building (
					building_id,
					district_id,
					building_name,
					floors)
				VALUES (@id, @dist_id, @name, @floors)";
			command.Parameters.AddWithValue("@id",      int.Parse(parts[0]));
			command.Parameters.AddWithValue("@dist_id", int.Parse(parts[1]));
			command.Parameters.AddWithValue("@name",    parts[2]);
			command.Parameters.AddWithValue("@floors",  parts[3]);
			command.ExecuteNonQuery();
		}
	}

	public void ImportDistrictCsv(string path)
	{
		using var connection = new SqliteConnection($"Data Source={_path}");

		connection.Open();

		string[] lines = File.ReadAllLines(path);

		for (int i = 1; i < lines.Length; i++)
		{
				string[] parts = lines[i].Split(';');
				if (parts.Length < 2) continue;

				var command = connection.CreateCommand();
				command.CommandText = @"
				INSERT INTO district (
					district_id, 
					district_name)
				VALUES (@id, @name)";
				command.Parameters.AddWithValue("@id",   int.Parse(parts[0]));
				command.Parameters.AddWithValue("@name", parts[1]);
				command.ExecuteNonQuery();
		}
	}

	public List<District> GetAllDistricts()
	{
		var result = new List<District>();
		using var connection = new SqliteConnection($"Data Source={_path}");
		connection.Open();

		var command = connection.CreateCommand();
		command.CommandText = "SELECT district_id, district_name FROM district ORDER BY district_id";
		using var reader = command.ExecuteReader();

		while (reader.Read())
		{
			result.Add(new District(
				reader.GetInt32(0),
				reader.GetString(1)
			));
		}
		return result;
	}

	public List<Building> GetAllBuildings()
	{
		var result = new List<Building>();
		using var connection = new SqliteConnection($"Data Source={_path}");
		connection.Open();

		var command = connection.CreateCommand();
		command.CommandText = "SELECT building_id, district_id, building_name, floors FROM building ORDER BY building_id";
		using var reader = command.ExecuteReader();

		while (reader.Read())
		{
			result.Add(new Building(
				reader.GetInt32(0),
				reader.GetInt32(1),
				reader.GetString(2),
				reader.GetInt32(3)
			));
		}
		return result;
	}

	public Building GetBuildingById(int id)
	{
		using var connection = new SqliteConnection($"Data Source={_path}");
		connection.Open();

		var command = connection.CreateCommand();
		command.CommandText = "SELECT building_id, district_id, building_name, floors FROM building WHERE building_id = @id";
		command.Parameters.AddWithValue("@id", id);
		using var reader = command.ExecuteReader();

		if (reader.Read())
		{
			return new Building(
				reader.GetInt32(0),
				reader.GetInt32(1),
				reader.GetString(2),
				reader.GetInt32(3)
			);
		}
		return null;
	}

	public void AddBuilding(Building building)
	{
		using var connection = new SqliteConnection($"Data Source={_path}");
		connection.Open();

		var command = connection.CreateCommand();
		command.CommandText = @"
			INSERT INTO building (district_id, building_name, floors)
			VALUES (@Id, @name, @floors)
		";
		command.Parameters.AddWithValue("@Id",     building.DistrictId);
		command.Parameters.AddWithValue("@name",   building.Name);
		command.Parameters.AddWithValue("@floors", building.Floors);
		command.ExecuteNonQuery();
	}

	public void UpdateBuilding(Building building)
	{
		using var connection = new SqliteConnection($"Data Source={_path}");
		connection.Open();

		var command = connection.CreateCommand();
		command.CommandText = @"
			UPDATE building
			SET district_id = @Id,
				building_name = @name,
				floors = @floors
			WHERE building_id = @id
		";
		command.Parameters.AddWithValue("@id", building.Id);
		command.Parameters.AddWithValue("@Id", building.DistrictId);
		command.Parameters.AddWithValue("@name", building.Name);
		command.Parameters.AddWithValue("@floors", building.Floors);
		command.ExecuteNonQuery();
	}

	public void DeleteBuilding(int id)
	{
		using var connection = new SqliteConnection($"Data Source={_path}");
		connection.Open();

		var command = connection.CreateCommand();
		command.CommandText = "DELETE FROM building WHERE building_id = @id";
		command.Parameters.AddWithValue("@id", id);
		command.ExecuteNonQuery();
	}

	public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
	{
		using var connection = new SqliteConnection($"Data Source={_path}");
		connection.Open();

		var command = connection.CreateCommand();
		command.CommandText = sql;
		using var reader = command.ExecuteReader();

		// Получаем имена столбцов
		string[] columns = new string[reader.FieldCount];
		for (int i = 0; i < reader.FieldCount; i++)
			columns[i] = reader.GetName(i);

		// Собираем строки данных
		var rows = new List<string[]>();
		while (reader.Read())
		{
			string[] row = new string[reader.FieldCount];
			for (int i = 0; i < reader.FieldCount; i++)
				row[i] = reader.GetValue(i)?.ToString() ?? "";
			rows.Add(row);
		}
		return (columns, rows);
	}

	public List<Building> GetBuildingsByDistrict(int districtId)
	{
		var result = new List<Building>();
		using var connection = new SqliteConnection($"Data Source={_path}");
		connection.Open();

		var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT building_id, district_id, building_name, floors
			FROM building
			WHERE district_id = @Id
			ORDER BY building_name
		";
		command.Parameters.AddWithValue("@Id", districtId);
		using var reader = command.ExecuteReader();

		while (reader.Read())
		{
			result.Add(new Building(
				reader.GetInt32(0),
				reader.GetInt32(1),
				reader.GetString(2),
				reader.GetInt32(3)
			));
		}
		return result;
	}

	public void ExportToCsv(string Path, string buildingPath)
	{
		// Экспорт микрорайонов
		var Lines = new List<string> { "district_id;district_name" };
		foreach (var m in GetAllDistricts())
			Lines.Add($"{m.Id};{m.Name}");
		File.WriteAllLines(Path, Lines);

		// Экспорт домов
		var buildingLines = new List<string> { "building_id;district_id;building_name;floors" };
		foreach (var b in GetAllBuildings())
			buildingLines.Add($"{b.Id};{b.DistrictId};{b.Name};{b.Floors}");
		File.WriteAllLines(buildingPath, buildingLines);
	}
}

