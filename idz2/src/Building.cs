/// <summary>
/// Жилой дом (основная таблица)
/// <\summary>

class Building
{
	private int _floors;

	public int Id { get; set; }
	public int DistrictId { get; set; }
	public string Name { get; set; }

	public int Floors
	{
		get => _floors;
		set
		{
			if (value < 1)
				throw new ArgumentException("Количество этажей не может быть меньше одного");
			_floors = value;
		}
	}

	public Building(int id, int districtId, string name, int floors)
	{
		Id = id;
		DistrictId = districtId;
		Name = name;
		Floors = floors;
       	}

	public Building() : this(0, 0, "", 1) { }

	public override string ToString() => $"{Id}, {Name}, микрорайон {DistrictId}, {Floors} этажей";
}

