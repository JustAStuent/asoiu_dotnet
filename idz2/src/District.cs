/// <summary>
/// Микрорайон (справочная таблица)
/// <\summary>

class District
{
	public int Id { get; set; }
	public string Name { get; set; }

	public District(int id, string name)
	{
		Id = id;
		Name = name;
	}

	public District() : this(0, "") { }

	public override string ToString() => $"{Name} - {Id}";
}

