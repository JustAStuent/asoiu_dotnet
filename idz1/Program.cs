using System;

class Program {
	static int DLDistance(string str1, string str2)
	{
		int[,] matrix = new int[str1.Length+1, str2.Length+1];

		for (int i = 0; i <= str1.Length; i++) { matrix[i,0] = i; }
		for (int j = 0; j <= str2.Length; j++) { matrix[0,j] = j; }

		for (int i = 1; i <= str1.Length; i++)
		{
			for (int j = 1; j <= str2.Length; j++)
			{
				int cost = str1[i-1] == str2[j-1] ? 0 : 1;
				int[] list = [
				    matrix[i-1, j] + 1,      // Вставка
				    matrix[i, j-1] + 1,      // Удаление
				    matrix[i-1, j-1] + cost  // Замена
				];
				matrix[i, j] = list.Min();

				// Проверка на возможность престановки
				if (i > 1 && j > 1 && str1[i-1] == str2[j-2] && str1[i-2] == str2[j-1]) {
					matrix[i, j] = Math.Min(matrix[i, j], matrix[i-2, j-2] + cost);
				}
			}
		}

		return matrix[str1.Length, str2.Length];
	}

	static void PrintDistance(string str1, string str2)
	{
		Console.WriteLine($"- {DLDistance(str1, str2)}: \"{str1}\" -> \"{str2}\"");
	}

	static void Main()
	{
		Console.WriteLine("\nБез изменений");
		PrintDistance("пример", "пример");

		Console.WriteLine("\nВставка 1го симаола");
		PrintDistance("пример", "1пример");
		PrintDistance("пример", "пример1");

		Console.WriteLine("\nВставка 2х символов");
		PrintDistance("пример", "12пример");
		PrintDistance("пример", "при12мер");
		PrintDistance("пример", "пример12");
		
		Console.WriteLine("\nВставка 3х символов");
		PrintDistance("пример", "1при2мер3");

		Console.WriteLine("\nЗамениа символов");
		PrintDistance("прИМер", "прМИер");

		Console.WriteLine("\nПример из условия");
		PrintDistance("ИВАНОВ", "БАННВО");
	}
}
