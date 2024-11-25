//rozvrhy :3
using System.Text.RegularExpressions;

//menu choices
Rozvrh? LoadedTimetable = null;
PrintableSections PrintSectionNext = 0;
bool loopApp = true;



while (loopApp)
{
    Console.Clear();
    PrintSectionNext = PrintSectionNext switch
    {
        PrintableSections.Hlavni => MenuHlavni(),
        PrintableSections.Zobrazit => MenuZobrazitRozvrh(),
        PrintableSections.Vytvorit => MenuVytvoritRozvrh(),
        PrintableSections.Upravit => MenuUpravitRozvrh(),
        _ => PrintableSections.EXIT
    };

    if (PrintSectionNext == PrintableSections.EXIT) loopApp = false;
}



//menu related funkce :)
//všechny vrací který menu se má zobrazit následovně
PrintableSections MenuHlavni()
{
    Console.WriteLine("Vítej v rozvrhové aplikaci :)");
    return GetMenuChoice("-----", "Zobrazit Rozvrh", "Vytvořit Rozvrh", "Ukončit Aplikaci") switch
    {
        1 => PrintableSections.Zobrazit,
        2 => PrintableSections.Vytvorit,
        _ => PrintableSections.EXIT
    };
}

PrintableSections MenuZobrazitRozvrh()
{
    if (LoadedTimetable == null)
    {
        Console.WriteLine("Který rozvrh chcete zobrazit? (nech prázdné pro návrat)\n---");
        int i = 0;
        string[] timetables = Rozvrh.GetTimetables();
        if (timetables.Length > 0)
        {
            foreach (var rozvrh in timetables)
            {
                ++i;
                Console.WriteLine($"{i}) {rozvrh}");
            }
        }
        else
        {
            Console.WriteLine("Ještě nemáte uložené žádné rozvrhy!");
            Console.ReadLine();
            return PrintableSections.Hlavni;
        }
        Console.WriteLine("---");

        while (true)
        {
            var input = Console.ReadLine();
            if (int.TryParse(input, out int choice))
            {
                if (choice > 0 && choice <= timetables.Length)
                {
                    LoadedTimetable = Rozvrh.Load(Rozvrh.SavePath + timetables[i - 1] + ".rozvrh");
                    break;
                }
            }
            else if (input == string.Empty) return PrintableSections.Hlavni;
            Console.WriteLine("Neplataný rozvrh, zkus to znovu!");
        }
        Console.Clear();
    }
    LoadedTimetable!.PrintTimetable();

    switch (GetMenuChoice("--", "Upravit rozvrh", "Smazat rozvrh", "Vybrat jiný rozvrh", "Vrátit se do menu"))
    {
        case 1:
            return PrintableSections.Upravit;
        case 2:
            //mazání rozvrhu
            Console.Clear();
            LoadedTimetable.PrintTimetable();
            Console.WriteLine("---\nOpravdu smazat tento rozvrh? (ano/ne)");
            while (true)
            {
                var choice = Console.ReadLine()!.Trim().ToLower();
                if (choice == "ano")
                {
                    Console.Clear();
                    if (!Rozvrh.Delete(Rozvrh.SavePath + LoadedTimetable!.Trida + ".rozvrh"))
                    {
                        Console.WriteLine("Nastala chyba při mazání rozvrhu!");
                        continue;
                    }
                    Console.WriteLine($"Rozvrh třídy {LoadedTimetable.Trida} smazán!");
                    LoadedTimetable = null;
                    Console.ReadLine();
                    return PrintableSections.Hlavni;
                }
                else if (choice == "ne") return PrintableSections.Zobrazit;
                else Console.WriteLine("Neplatná volba, zkus to znovu!");
            }
        case 3:
            LoadedTimetable = null;
            return PrintableSections.Zobrazit;
        default:
            return PrintableSections.Hlavni;
    }
}

PrintableSections MenuVytvoritRozvrh()
{
    Console.WriteLine("Zadej název třídy, pro kterou rozvrh bude (pouze alfanumerické znaky) (nech prázdné pro návrat):");
    while (true)
    {
        string nazevTridy = Console.ReadLine()!;
        if (nazevTridy == string.Empty) return PrintableSections.Hlavni;
        else if (nazevTridy.All(char.IsLetterOrDigit))
        {
            LoadedTimetable = new(nazevTridy); //načte rozvrh do proměnný 
            LoadedTimetable.Save(); //aby to nemusel hledat
            return PrintableSections.Zobrazit;
        }
        else Console.WriteLine("\nNeplatný název, zkus to znovu!");
    }
}

PrintableSections MenuUpravitRozvrh()
{
    //kopírování
    Rozvrh tmpTimetable = new(LoadedTimetable!.Trida);
    for (int y = 0; y < 5; y++)
    {
        for (int x = 0; x < 10; x++)
        {
            tmpTimetable.Hodiny[x, y] = LoadedTimetable.Hodiny[x, y];
        }
    }

    while (true)
    {
        Console.Clear();
        tmpTimetable!.PrintTimetable();

        switch (GetMenuChoice("--", "Změnit název třídy", "Změnit hodinu", "Uložit změny a vrátit se", "Vrátit se bez uložení změn"))
        {
            case 1:
                //změna názvu třídy
                Console.Clear();
                Console.WriteLine("Zadej nový název třídy (pouze alfanumerické znaky) (nech prázdné pro návrat):");
                while (true)
                {
                    string nazev = Console.ReadLine()!;
                    if (nazev == string.Empty) break;
                    else if (nazev.All(char.IsLetterOrDigit))
                    {
                        tmpTimetable.Trida = nazev;
                        break;
                    }
                    else Console.WriteLine("Neplatný název třídy, zkus to znovu!");
                }
                break;
            case 2:
                //změna hodiny
                Console.Clear();
                tmpTimetable!.PrintTimetable();
                Console.WriteLine("Kterou hodinu chcete změnit? (formát: hodina den) (den napište jako číslo 1 - 5) (nechte prázdné pro návrat): ");
                while (true)
                {
                    var matches = Regex.Matches(Console.ReadLine()!, @"(\d+)");
                    if (matches.Count == 2)
                    {
                        if (!(int.Parse(matches[0].Value) > 0 && int.Parse(matches[0].Value) < 10 &&
                            int.Parse(matches[1].Value) > 0 && int.Parse(matches[1].Value) < 6))
                        {
                            Console.WriteLine("Tato hodina neexistuje, zkus to znovu!");
                            continue;
                        }
                        Console.Clear();
                        Console.WriteLine("Zadej název nové hodiny (1 - 4 písmena) (/ pro vyprázdnění pole; nech prázdné pro návrat):");
                        while (true)
                        {
                            string novaHodina = Console.ReadLine()!;
                            if (novaHodina.All(char.IsLetter) && novaHodina.Length > 0 && novaHodina.Length < 5) {}
                            else if (novaHodina == "/") novaHodina = string.Empty;
                            else if (novaHodina == string.Empty) break;
                            else 
                            {
                                Console.WriteLine("Neplatný formát hodiny, zkus to znovu!");
                                continue;
                            }

                            tmpTimetable.Hodiny[int.Parse(matches[0].Value) - 1, int.Parse(matches[1].Value) - 1] = novaHodina;
                            break;
                        }
                        break;
                    }
                    else Console.WriteLine("Špatný formát, zkus to znovu!");
                }
                break;
            case 3:
                //uložit a vrátit se
                LoadedTimetable = tmpTimetable;
                if (!LoadedTimetable.Save())
                {
                    Console.WriteLine("Nastal problém při ukládání rozvrhu!");
                    break;
                }
                return PrintableSections.Zobrazit;
            case 4:
                //neukládat a vrátit se
                Console.Clear();
                Console.WriteLine("Opravdu se vrátit bez uložení změn v rozvrhu? (ano/ne)");
                while (true)
                {
                    var choice = Console.ReadLine()!.Trim().ToLower();
                    if (choice == "ano") return PrintableSections.Zobrazit;
                    else if (choice == "ne") break;
                    else Console.WriteLine("Neplatná volba, zkus to znovu!");
                }
                break;
        }
    }
}



int GetMenuChoice(string headWith, params string[] options)
{
    Console.WriteLine(headWith);
    for (int i = 0; i < options.Length; i++)
    {
        Console.WriteLine($"{i + 1}) {options[i]}");
    }
    Console.WriteLine(headWith);

    Console.WriteLine("Zadej číslo vybrané možnosti:");
    while (true)
    {
        if (int.TryParse(Console.ReadLine(), out int input))
        {
            if (input > 0 && input <= options.Length) return input;
        }
        Console.WriteLine("Neplatný výběr! Zkus to znovu:");
    }
}
enum PrintableSections
{
    Hlavni, Zobrazit, Vytvorit, Upravit, EXIT
}



class Rozvrh
{
    public string Trida;
    public string[,] Hodiny;
    public static string SavePath { get {
            const string savePath = "../../../rozvrhy/";
            Directory.CreateDirectory(savePath);
            return savePath;
        }
    }

    public Rozvrh(string trida)
    {
        Trida = trida;
        Hodiny = new string[10, 5];
    }

    //operace s rozvrhem
    public static string[] GetTimetables()
    {
        try
        {
            List<string> rozvrhy = new();
            foreach (var rozvrh in Directory.GetFiles(SavePath))
            {
                if (Path.GetExtension(rozvrh) == ".rozvrh") rozvrhy.Add(Path.GetFileNameWithoutExtension(rozvrh));
            }
            return rozvrhy.ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine("Nastal problém při načítání rozvrhů: "+e.Message);
            return Array.Empty<string>();
        }
    }
    public void PrintTimetable()
    {
        string[] Dny = {"Po", "Út", "St", "Čt", "Pá"};
        Console.WriteLine($"===== ROZVRH TŘÍDY: {Trida} =====\n");
        for (int y = -1; y <= 5; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                if (x == 0)
                {
                    if (y == -1) Console.Write("     ");
                    else if (y == 0) Console.Write("   +-");
                    else Console.Write(Dny[y - 1]+" | ");
                }
                else if (y == 0) Console.Write("-----");
                else if (y == -1) Console.Write($"  {x}".PadRight(5));
                else Console.Write((Hodiny[x - 1, y - 1] ?? string.Empty).PadRight(5));
            }
            Console.WriteLine();
        }
    }

    //souborová manipulace
    public bool Save()
    {
        try
        {
            string saveFile = SavePath+Trida+".rozvrh";
            List<string> saveCotents = new();
            for (int y = 0; y < 5; y++)
            {   
                List<string> tmp = new();
                for (int x = 0; x < 10; x++)
                {
                    tmp.Add(Hodiny[x, y]);
                }
                saveCotents.Add(string.Join(',', tmp));
            }
            File.WriteAllText(saveFile, string.Join('\n', saveCotents), System.Text.Encoding.UTF8);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Nastal problém při ukládání rozvrhu: "+e.Message);
            return false;
        }
    }
    public static Rozvrh? Load(string saveFile)
    {
        try
        {
            Rozvrh tmpRozvrh = new(Path.GetFileNameWithoutExtension(saveFile));
            string[,] hodiny = new string[10, 5];

            string[] tmp = File.ReadAllText(saveFile, System.Text.Encoding.UTF8).Split("\n");
            for (int y = 0; y < 5; y++)
            {   
                string[] tmpLine = tmp[y].Split(',');
                for (int x = 0; x < 10; x++)
                {
                    hodiny[x, y] = tmpLine[x];
                }
            }

            tmpRozvrh.Hodiny = hodiny;
            return tmpRozvrh;
        }
        catch (Exception e)
        {
            Console.WriteLine("Nastal problém při načítání rozvrhu: "+e.Message);
            return null;
        }
    }
    public static bool Delete(string saveFile)
    {
        try
        {
            if (File.Exists(saveFile))
            {
                File.Delete(saveFile);
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Nastal problém při mazání rozvrhu: "+e.Message);
        }
        return false;
    }
}