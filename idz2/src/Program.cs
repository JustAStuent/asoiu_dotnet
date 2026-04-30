using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HousingEstate
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			Console.InputEncoding = Encoding.UTF8;

			// Пути к файлам БД и CSV
			string dbPath = "dat/database.db";
			string csv = Path.Combine(AppContext.BaseDirectory, "dat/district.csv");
			string buildingCsv = Path.Combine(AppContext.BaseDirectory, "dat/building.csv");

			// Инициализация менеджера БД и загрузка данных
			var db = new DatabaseManager(dbPath);

			Console.WriteLine();

			// Спасибо ИИ за красивый CLI/TUI
			string choice;
			do
			{
				Console.WriteLine("╔═══════════════════════════════════════╗");
				Console.WriteLine("║ УПРАВЛЕНИЕ ЖИЛЫМИ ДОМАМИ              ║");
				Console.WriteLine("╠═══════════════════════════════════════╣");
				Console.WriteLine("║ 1 — Показать все микрорайоны          ║");
				Console.WriteLine("║ 2 — Показать все дома                 ║");
				Console.WriteLine("║ 3 — Добавить дом                      ║");
				Console.WriteLine("║ 4 — Редактировать дом                 ║");
				Console.WriteLine("║ 5 — Удалить дом                       ║");
				Console.WriteLine("║ 6 — Отчёты                            ║");
				Console.WriteLine("║ 0 — Выход                             ║");
				Console.WriteLine("╚═══════════════════════════════════════╝");
				Console.Write("Ваш выбор: ");

				choice = Console.ReadLine()?.Trim() ?? "";
				Console.WriteLine();

				switch (choice)
				{
					case "1": ShowDistricts(db); break;
					case "2": ShowBuildings(db); break;
					case "3": AddBuilding(db); break;
					case "4": EditBuilding(db); break;
					case "5": DeleteBuilding(db); break;
					case "6": ReportsMenu(db); break;
					case "0": Console.WriteLine("До свидания!"); break;
					default: Console.WriteLine("Неверный пункт меню."); break;
				}
				Console.WriteLine();
			} while (choice != "0");
		}

		//  Пункты меню
		static void ShowDistricts(DatabaseManager db)
		{
			Console.WriteLine("---- Все микрорайоны ----");
			var items = db.GetAllDistricts();
			foreach (var m in items)
				Console.WriteLine($"  {m}");
			Console.WriteLine($"Итого: {items.Count}");
		}

		static void ShowBuildings(DatabaseManager db)
		{
			Console.WriteLine("---- Все жилые дома ----");
			var items = db.GetAllBuildings();
			foreach (var b in items)
				Console.WriteLine($"  {b}");
			Console.WriteLine($"Итого: {items.Count}");
		}

		static void AddBuilding(DatabaseManager db)
		{
			Console.WriteLine("---- Добавление дома ----");
			Console.WriteLine("Доступные микрорайоны:");
			var s = db.GetAllDistricts();
			foreach (var m in s)
				Console.WriteLine($"  {m}");

			Console.Write("ID микрорайона: ");
			if (!int.TryParse(Console.ReadLine(), out int id))
			{
				Console.WriteLine("Ошибка: введите целое число.");
				return;
			}

			Console.Write("Название дома (адрес): ");
			string name = Console.ReadLine()?.Trim() ?? "";
			if (string.IsNullOrEmpty(name))
			{
				Console.WriteLine("Ошибка: название не может быть пустым.");
				return;
			}

			Console.Write("Количество этажей: ");
			if (!int.TryParse(Console.ReadLine(), out int floors))
			{
				Console.WriteLine("Ошибка: введите целое число.");
				return;
			}

			try
			{
				var building = new Building(0, id, name, floors);
				db.AddBuilding(building);
				Console.WriteLine("Дом успешно добавлен.");
			}
			catch (ArgumentException ex)
			{
				Console.WriteLine($"Ошибка: {ex.Message}");
			}
		}

		static void EditBuilding(DatabaseManager db)
		{
			Console.WriteLine("---- Редактирование дома ----");
			Console.Write("Введите ID дома: ");
			if (!int.TryParse(Console.ReadLine(), out int id))
			{
				Console.WriteLine("Ошибка: введите целое число.");
				return;
			}

			var building = db.GetBuildingById(id);
			if (building == null)
			{
				Console.WriteLine($"Дом с ID={id} не найден.");
				return;
			}

			Console.WriteLine($"Текущие данные: {building}");
			Console.WriteLine("(Нажмите Enter, чтобы оставить значение без изменений)");

			// Название
			Console.Write($"Название дома [{building.Name}]: ");
			string input = Console.ReadLine()?.Trim() ?? "";
			if (!string.IsNullOrEmpty(input))
				building.Name = input;

			// Микрорайон
			Console.Write($"ID микрорайона [{building.DistrictId}]: ");
			input = Console.ReadLine()?.Trim() ?? "";
			if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int newMicroId))
				building.DistrictId = newMicroId;

			// Этажи
			Console.Write($"Количество этажей [{building.Floors}]: ");
			input = Console.ReadLine()?.Trim() ?? "";
			if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int newFloors))
			{
				try
				{
					building.Floors = newFloors; // валидация в set-аксессоре
				}
				catch (ArgumentException ex)
				{
					Console.WriteLine($"Ошибка: {ex.Message}");
					return;
				}
			}

			db.UpdateBuilding(building);
			Console.WriteLine("Данные обновлены.");
		}

		static void DeleteBuilding(DatabaseManager db)
		{
			Console.WriteLine("---- Удаление дома ----");
			Console.Write("Введите ID дома: ");
			if (!int.TryParse(Console.ReadLine(), out int id))
			{
				Console.WriteLine("Ошибка: введите целое число.");
				return;
			}

			var building = db.GetBuildingById(id);
			if (building == null)
			{
				Console.WriteLine($"Дом с ID={id} не найден.");
				return;
			}

			Console.Write($"Удалить дом «{building.Name}»? (да/нет): ");
			string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
			if (confirm == "да")
			{
				db.DeleteBuilding(id);
				Console.WriteLine("Дом удалён.");
			}
			else
				Console.WriteLine("Удаление отменено.");
		}

		//  Меню отчётов
		static void ReportsMenu(DatabaseManager db)
		{
			string choice;
			do
			{
				Console.WriteLine("--- Отчёты ---");
				Console.WriteLine(" 1 - Список домов с названиями микрорайонов");
				Console.WriteLine(" 2 - Количество домов по микрорайонам");
				Console.WriteLine(" 3 - Среднее количество этажей по микрорайонам");
				Console.WriteLine(" 0 - Назад");
				Console.Write("Ваш выбор: ");
				choice = Console.ReadLine()?.Trim() ?? "";

				switch (choice)
				{
					case "1": Report1_BuildingsWithDistricts(db); break;
					case "2": Report2_CountByDistrict(db); break;
					case "3": Report3_AvgFloorsByDistrict(db); break;
					case "0": break;
					default: Console.WriteLine("Неверный пункт."); break;
				}
				Console.WriteLine();
			} while (choice != "0");
		}

		// Отчёт 1: список домов + названия микрорайонов (JOIN)
		static void Report1_BuildingsWithDistricts(DatabaseManager db)
		{
			new ReportBuilder(db)
				.Query(@"SELECT b.building_name, m.district_name, b.floors
					 FROM building b
					 JOIN district m ON b.district_id = m.district_id
					 ORDER BY b.building_name")
				.Title("Жилые дома по микрорайонам")
				.Header("Дом", "Микрорайон", "Этажей")
				.ColumnWidths(30, 20, 10)
				.Footer("Всего записей")
				.Print();
		}

		// Отчёт 2: количество домов по микрорайонам (GROUP BY COUNT)
		static void Report2_CountByDistrict(DatabaseManager db)
		{
			new ReportBuilder(db)
				.Query(@"SELECT m.district_name, COUNT(*) AS cnt
					 FROM building b
					 JOIN district m ON b.district_id = m.district_id
					 GROUP BY m.district_name
					 ORDER BY m.district_name")
				.Title("Количество домов по микрорайонам")
				.Header("Микрорайон", "Количество домов")
				.ColumnWidths(25, 15)
				.Print();
		}

		// Отчёт 3: средняя этажность по микрорайонам (AVG)
		static void Report3_AvgFloorsByDistrict(DatabaseManager db)
		{
			new ReportBuilder(db)
				.Query(@"SELECT m.district_name, ROUND(AVG(b.floors), 1) AS avg_floors
					 FROM building b
					 JOIN district m ON b.district_id = m.district_id
					 GROUP BY m.district_name
					 ORDER BY avg_floors DESC")
				.Title("Среднее количество этажей по микрорайонам")
				.Header("Микрорайон", "Среднее этажей")
				.ColumnWidths(25, 15)
				.Print();
		}
	}
}
