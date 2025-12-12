using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Cafe_Management_System
{
    class Program
    {
        static void Main()
        {
            bool exitApp = false;
            while (!exitApp)
            {
                Console.Clear();
                Console.WriteLine("--- Café Management System ---\n");

                // file pathfolder
                string folderPath = @"C:\Users\JAMES PATRICK\Documents\BSCPE-2 FINAL CODE CAFE MANAGEMENT SYSTEM\FINAL PROJECT CAFE MANAGEMENT SYSTEM\FILE FOLDER PATH";

                FileManager fileManager = new FileManager(folderPath);
                fileManager.CreateFiles();

                ItemManager itemManager = new ItemManager(fileManager);
                SaleManager saleManager = new SaleManager(fileManager);
                UserInterface ui = new UserInterface(itemManager, saleManager, fileManager);

                Console.Write("Enter username: ");
                string username = Console.ReadLine()?.Trim();
                Console.Write("Enter password: ");
                string password = ReadPassword();

                if (username == "owner" && password == "owner123")
                {
                    ui.OwnerMenu(ref exitApp);
                }
                else if (username == "cashier" && password == "cashier123")
                {
                    ui.CashierMenu(ref exitApp);
                }
                else
                {
                    Console.WriteLine("Invalid login! Press Enter to try again.");
                    Console.ReadLine();
                }
            }
        }

        private static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }
    }

    // ---------------- Item ----------------
    public class Item
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }

        public Item() { }
        public Item(string name, double price, int stock)
        {
            Name = name;
            Price = price;
            Stock = stock;
        }
    }

    // ---------------- ItemManager ----------------
    public class ItemManager
    {
        private List<Item> items = new List<Item>();
        private FileManager fileManager;

        public ItemManager(FileManager fileManager)
        {
            this.fileManager = fileManager;
            LoadItems();
        }

        private void LoadItems()
        {
            var loadedItems = fileManager.LoadItemsFromFile();
            if (loadedItems != null && loadedItems.Any())
            {
                items = loadedItems;
            }
            else
            {
                // default items
                items.Add(new Item("Americano", 80, 20));
                items.Add(new Item("Cappuccino", 120, 15));
                items.Add(new Item("Latte", 110, 15));
                items.Add(new Item("Caramel Macchiato", 130, 10));
                items.Add(new Item("Chocolate Cake", 150, 8));
                items.Add(new Item("Croissant", 75, 25));
                fileManager.SaveItemsToFile(items);
            }
        }

        public List<Item> GetItems() => items;

        public void ViewItems()
        {
            Console.WriteLine("\n--- Café Items ---");
            for (int i = 0; i < items.Count; i++)
                Console.WriteLine($"{i + 1}. {items[i].Name} - PHP {items[i].Price} ({items[i].Stock} in stock)");
        }

        public void AddItem(string name, double price, int stock)
        {
            items.Add(new Item(name, price, stock));
            Console.WriteLine("Item added successfully!");
        }

        public bool EditItem(int index, double newPrice, int newStock)
        {
            if (index < 0 || index >= items.Count)
            {
                Console.WriteLine("Invalid item number!");
                return false;
            }
            items[index].Price = newPrice;
            items[index].Stock = newStock;
            Console.WriteLine("Item updated successfully!");
            return true;
        }

        public bool DeleteItem(int index)
        {
            if (index < 0 || index >= items.Count)
            {
                Console.WriteLine("Invalid item number!");
                return false;
            }
            items.RemoveAt(index);
            Console.WriteLine("Item deleted successfully!");
            return true;
        }

        public Item GetItem(int index) => (index >= 0 && index < items.Count) ? items[index] : null;

        public bool ReduceStock(int index, int quantity)
        {
            if (index < 0 || index >= items.Count || quantity > items[index].Stock)
            {
                Console.WriteLine("Invalid item or not enough stock!");
                return false;
            }
            items[index].Stock -= quantity;
            return true;
        }
    }

    // ---------------- Sale ----------------
    public class Sale
    {
        public DateTime Date { get; set; }
        public List<(string ItemName, int Quantity, double Subtotal)> Items { get; set; }
        public double GrandTotal { get; set; }
        public double Cash { get; set; }
        public double Change { get; set; }
        public string PaymentMethod { get; set; } // Cash, GCash, Maya

        public Sale()
        {
            Date = DateTime.Now;
            Items = new List<(string, int, double)>();
            GrandTotal = 0;
            Cash = 0;
            Change = 0;
            PaymentMethod = "Cash";
        }
    }

    // ---------------- SaleManager ----------------
    public class SaleManager
    {
        private List<Sale> sales = new List<Sale>();
        private FileManager fileManager;

        public SaleManager(FileManager fm)
        {
            fileManager = fm;
            LoadSales();
        }

        public void RecordSale(Sale sale)
        {
            sales.Add(sale);
            fileManager.SaveSaleToFile(sale);
        }

        public void ViewReport()
        {
            if (sales.Count == 0)
            {
                Console.WriteLine("No sales recorded yet.");
                return;
            }

            Console.WriteLine("\n--- SALES REPORT ---");
            double total = 0;

            foreach (var sale in sales)
            {
                Console.WriteLine($"\nDate: {sale.Date.ToString("MM/dd/yyyy")}");
                foreach (var item in sale.Items)
                {
                    Console.WriteLine($"{item.ItemName} x{item.Quantity} = PHP {item.Subtotal}");
                }
                Console.WriteLine($"Payment: {sale.PaymentMethod}");
                Console.WriteLine($"Grand Total: PHP {sale.GrandTotal}");
                Console.WriteLine("------------------------------");

                total += sale.GrandTotal;
            }

            Console.WriteLine($"\nTOTAL SALES: PHP {total}");
        }

        public void ViewMonthlyReport(int month, int year)
        {
            var report = sales.Where(s => s.Date.Month == month && s.Date.Year == year).ToList();
            Console.WriteLine($"\n--- MONTHLY REPORT ({month}/{year}) ---");

            if (!report.Any())
            {
                Console.WriteLine("No sales for this month.");
                return;
            }

            double total = report.Sum(s => s.GrandTotal);
            Console.WriteLine($"Total Monthly Sales: PHP {total}");
            Console.WriteLine("\nDetails:");
            foreach (var s in report)
                Console.WriteLine($"{s.Date.ToString("MM/dd/yyyy")}: PHP {s.GrandTotal} ({s.PaymentMethod})");
        }

        public void ViewYearlyReport(int year)
        {
            var report = sales.Where(s => s.Date.Year == year).ToList();
            Console.WriteLine($"\n--- YEARLY REPORT ({year}) ---");

            if (!report.Any())
            {
                Console.WriteLine("No sales for this year.");
                return;
            }

            double total = report.Sum(s => s.GrandTotal);
            Console.WriteLine($"Total Yearly Sales: PHP {total}");
            Console.WriteLine("\nDetails:");
            foreach (var s in report)
                Console.WriteLine($"{s.Date.ToString("MM/dd/yyyy")}: PHP {s.GrandTotal} ({s.PaymentMethod})");
        }

        private void LoadSales()
        {
            string salesFile = Path.Combine(fileManager.GetFolderPath(), "sales.txt");
            if (!File.Exists(salesFile)) return;

            var lines = File.ReadAllLines(salesFile);
            Sale sale = null;

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("DATE:"))
                {
                    if (sale != null)
                        sales.Add(sale);

                    sale = new Sale();
                    var dateStr = line.Replace("DATE:", "").Trim();
                    if (DateTime.TryParseExact(dateStr, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d))
                        sale.Date = d;
                    else if (DateTime.TryParse(dateStr, out DateTime d2))
                        sale.Date = d2;
                }
                else if (line.StartsWith("ITEM:"))
                {
                    var parts = line.Replace("ITEM:", "").Trim().Split('|');
                    if (parts.Length >= 3 &&
                        int.TryParse(parts[1], out int qty) &&
                        double.TryParse(parts[2], out double sub))
                    {
                        sale.Items.Add((parts[0], qty, sub));
                    }
                }
                else if (line.StartsWith("GRANDTOTAL:"))
                {
                    double.TryParse(line.Replace("GRANDTOTAL:", "").Trim(), out double gt);
                    sale.GrandTotal = gt;
                }
                else if (line.StartsWith("CASH:"))
                {
                    double.TryParse(line.Replace("CASH:", "").Trim(), out double cash);
                    sale.Cash = cash;
                }
                else if (line.StartsWith("CHANGE:"))
                {
                    double.TryParse(line.Replace("CHANGE:", "").Trim(), out double ch);
                    sale.Change = ch;
                }
                else if (line.StartsWith("PAYMENT:"))
                {
                    sale.PaymentMethod = line.Replace("PAYMENT:", "").Trim();
                }
                // ignore separators
            }

            if (sale != null)
                sales.Add(sale);
        }

        public void GenerateReceipt(Sale sale, string folderPath)
        {
            string receiptFolder = Path.Combine(folderPath, "Receipt");
            if (!Directory.Exists(receiptFolder)) Directory.CreateDirectory(receiptFolder);

            string fileName = $"Receipt_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string path = Path.Combine(receiptFolder, fileName);

            using (StreamWriter r = new StreamWriter(path))
            {
                r.WriteLine("----------- CAFÉ RECEIPT -----------");
                r.WriteLine($"Date: {sale.Date.ToString("MM/dd/yyyy")}");
                r.WriteLine();
                r.WriteLine($"{"Item",-20} {"Qty",5} {"Subtotal",12}");
                r.WriteLine("--------------------------------------------");

                foreach (var item in sale.Items)
                    r.WriteLine($"{item.ItemName,-20} {item.Quantity,5} {item.Subtotal,12}");

                r.WriteLine("--------------------------------------------");
                r.WriteLine($"Grand Total: PHP {sale.GrandTotal}");
                r.WriteLine($"Payment: {sale.PaymentMethod}");
                if (sale.PaymentMethod == "Cash")
                {
                    r.WriteLine($"Cash:        PHP {sale.Cash}");
                    r.WriteLine($"Change:      PHP {sale.Change}");
                }
                else
                {
                    r.WriteLine("Paid (No change needed)");
                }
                r.WriteLine("--------------------------------------------");
                r.WriteLine("Thank you for your purchase!");
            }

            Console.WriteLine($"\nReceipt saved: {path}");
        }

        public void ShowReceiptOnScreen(Sale sale)
        {
            Console.WriteLine("\n----------- CAFÉ RECEIPT -----------");
            Console.WriteLine($"Date: {sale.Date.ToString("MM/dd/yyyy")}");
            Console.WriteLine();
            Console.WriteLine($"{"Item",-20} {"Qty",5} {"Subtotal",12}");
            Console.WriteLine("--------------------------------------------");

            foreach (var item in sale.Items)
                Console.WriteLine($"{item.ItemName,-20} {item.Quantity,5} {item.Subtotal,12}");

            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"Grand Total: PHP {sale.GrandTotal}");
            Console.WriteLine($"Payment: {sale.PaymentMethod}");
            if (sale.PaymentMethod == "Cash")
            {
                Console.WriteLine($"Cash:  PHP {sale.Cash}");
                Console.WriteLine($"Change: PHP {sale.Change}");
            }
            else
            {
                Console.WriteLine("Paid (No change needed)");
            }
            Console.WriteLine("--------------------------------------------");
        }
    }

    // ---------------- FileManager ----------------
    public class FileManager
    {
        private string folderPath;
        private string salesFile;
        private string itemsFile;

        public FileManager(string folderPath)
        {
            this.folderPath = folderPath;
            this.salesFile = Path.Combine(folderPath, "sales.txt");
            this.itemsFile = Path.Combine(folderPath, "items.txt");
        }

        public string GetFolderPath() => folderPath;

        public void CreateFiles()
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                if (!File.Exists(salesFile))
                    File.WriteAllText(salesFile, "");

                if (!File.Exists(itemsFile))
                    File.WriteAllText(itemsFile, "Name|Price|Stock"); // header
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating files: " + ex.Message);
            }
        }

        public void SaveItemsToFile(List<Item> items)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(itemsFile, false))
                {
                    file.WriteLine("Name|Price|Stock");
                    foreach (var it in items)
                        file.WriteLine($"{it.Name}|{it.Price}|{it.Stock}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving items: " + ex.Message);
            }
        }

        public List<Item> LoadItemsFromFile()
        {
            var items = new List<Item>();
            if (!File.Exists(itemsFile)) return items;

            var lines = File.ReadAllLines(itemsFile);
            foreach (var line in lines.Skip(1)) // skip header
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('|');
                if (parts.Length >= 3)
                {
                    var name = parts[0];
                    if (double.TryParse(parts[1], out double price) && int.TryParse(parts[2], out int stock))
                    {
                        items.Add(new Item(name, price, stock));
                    }
                }
            }
            return items;
        }

        // Save sale in simple parseable format:
        // DATE: MM/dd/yyyy
        // ITEM: name|qty|subtotal
        // PAYMENT: method
        // GRANDTOTAL: ...
        // CASH: ...
        // CHANGE: ...
        // ---------------------------------------
        public void SaveSaleToFile(Sale sale)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(salesFile, true))
                {
                    file.WriteLine($"DATE: {sale.Date.ToString("MM/dd/yyyy")}");
                    foreach (var item in sale.Items)
                    {
                        file.WriteLine($"ITEM: {item.ItemName}|{item.Quantity}|{item.Subtotal}");
                    }
                    file.WriteLine($"PAYMENT: {sale.PaymentMethod}");
                    file.WriteLine($"GRANDTOTAL: {sale.GrandTotal}");
                    file.WriteLine($"CASH: {sale.Cash}");
                    file.WriteLine($"CHANGE: {sale.Change}");
                    file.WriteLine("---------------------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving sale: " + ex.Message);
            }
        }
    }

    // ---------------- UserInterface ----------------
    public class UserInterface
    {
        private ItemManager itemManager;
        private SaleManager saleManager;
        private FileManager fileManager;

        public UserInterface(ItemManager itemManager, SaleManager saleManager, FileManager fileManager)
        {
            this.itemManager = itemManager;
            this.saleManager = saleManager;
            this.fileManager = fileManager;
        }

        public void OwnerMenu(ref bool exitApp)
        {
            int choice;
            do
            {
                Console.Clear();
                Console.WriteLine("---- OWNER MENU ----");
                Console.WriteLine("[1] View Items");
                Console.WriteLine("[2] Add Item");
                Console.WriteLine("[3] Edit Item");
                Console.WriteLine("[4] Delete Item");
                Console.WriteLine("[5] View Sales Report");
                Console.WriteLine("[6] View Monthly Sales Report");
                Console.WriteLine("[7] View Yearly Sales Report");
                Console.WriteLine("[8] Back to Main Menu");
                Console.WriteLine("[9] Exit Application");
                Console.Write("Enter choice: ");
                if (!int.TryParse(Console.ReadLine(), out choice)) choice = -1;

                switch (choice)
                {
                    case 1:
                        itemManager.ViewItems();
                        break;
                    case 2:
                        AddItemUI();
                        break;
                    case 3:
                        EditItemUI();
                        break;
                    case 4:
                        DeleteItemUI();
                        break;
                    case 5:
                        saleManager.ViewReport();
                        break;
                    case 6:
                        Console.Write("Enter month (1-12): ");
                        if (!int.TryParse(Console.ReadLine(), out int month) || month < 1 || month > 12) month = DateTime.Now.Month;
                        Console.Write("Enter year: ");
                        if (!int.TryParse(Console.ReadLine(), out int year)) year = DateTime.Now.Year;
                        saleManager.ViewMonthlyReport(month, year);
                        break;
                    case 7:
                        Console.Write("Enter year: ");
                        if (!int.TryParse(Console.ReadLine(), out int y)) y = DateTime.Now.Year;
                        saleManager.ViewYearlyReport(y);
                        break;
                    case 8:
                        return;
                    case 9:
                        exitApp = true;
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }

                if (choice != 8 && choice != 9)
                {
                    Console.WriteLine("\nPress Enter to continue...");
                    Console.ReadLine();
                }

            } while (choice != 8 && choice != 9);
        }

        public void CashierMenu(ref bool exitApp)
        {
            int choice;
            do
            {
                Console.Clear();
                Console.WriteLine("--- CASHIER MENU ---");
                Console.WriteLine("[1] Record Sale");
                Console.WriteLine("[2] View Items");
                Console.WriteLine("[3] Back to Main Menu");
                Console.WriteLine("[4] Exit Application");
                Console.Write("Enter choice: ");
                if (!int.TryParse(Console.ReadLine(), out choice)) choice = -1;

                switch (choice)
                {
                    case 1: RecordSaleUI(); break;
                    case 2: itemManager.ViewItems(); break;
                    case 3: return;
                    case 4: exitApp = true; return;
                    default: Console.WriteLine("Invalid choice."); break;
                }

                if (choice != 3 && choice != 4)
                {
                    Console.WriteLine("\nPress Enter to continue...");
                    Console.ReadLine();
                }

            } while (choice != 3 && choice != 4);
        }

        private void AddItemUI()
        {
            Console.Write("Enter new item name: ");
            string name = Console.ReadLine()?.Trim();
            Console.Write("Enter price: ");
            if (!double.TryParse(Console.ReadLine(), out double price)) price = 0;
            Console.Write("Enter stock: ");
            if (!int.TryParse(Console.ReadLine(), out int stock)) stock = 0;

            itemManager.AddItem(name, price, stock);
            fileManager.SaveItemsToFile(itemManager.GetItems());
        }

        private void EditItemUI()
        {
            itemManager.ViewItems();
            Console.Write("Enter item number to edit: ");
            if (!int.TryParse(Console.ReadLine(), out int num)) return;
            num = num - 1;
            Console.Write("Enter new price: ");
            if (!double.TryParse(Console.ReadLine(), out double price)) price = 0;
            Console.Write("Enter new stock: ");
            if (!int.TryParse(Console.ReadLine(), out int stock)) stock = 0;

            if (itemManager.EditItem(num, price, stock))
                fileManager.SaveItemsToFile(itemManager.GetItems());
        }

        private void DeleteItemUI()
        {
            itemManager.ViewItems();
            Console.Write("Enter item number to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int num)) return;
            num = num - 1;

            if (itemManager.DeleteItem(num))
                fileManager.SaveItemsToFile(itemManager.GetItems());
        }

        // MULTI-ITEM SALE WITH PAYMENT (Cash / GCash / Maya) 
        private void RecordSaleUI()
        {
            Sale sale = new Sale();
            bool addMore = true;

            while (addMore)
            {
                itemManager.ViewItems();
                Console.Write("Enter item number: ");
                if (!int.TryParse(Console.ReadLine(), out int num))
                {
                    Console.WriteLine("Invalid input. Cancelling sale.");
                    return;
                }
                num = num - 1;

                Item item = itemManager.GetItem(num);
                if (item == null)
                {
                    Console.WriteLine("Invalid item selected. Cancelling sale.");
                    return;
                }

                Console.Write("Enter quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
                {
                    Console.WriteLine("Invalid quantity. Cancelling sale.");
                    return;
                }

                if (!itemManager.ReduceStock(num, qty))
                {
                    Console.WriteLine("Not enough stock!");
                    return;
                }

                double subtotal = qty * item.Price;
                sale.Items.Add((item.Name, qty, subtotal));
                sale.GrandTotal += subtotal;

                Console.Write("Add another item? (y/n): ");
                string resp = Console.ReadLine()?.Trim().ToLower();
                addMore = resp == "y" || resp == "yes";
            }

            Console.WriteLine($"\nGRAND TOTAL: PHP {sale.GrandTotal}");

            // Payment method selection
            Console.WriteLine("Select payment method:");
            Console.WriteLine("[1] Cash");
            Console.WriteLine("[2] GCash");
            Console.WriteLine("[3] Maya");
            Console.Write("Choice: ");
            string pmChoice = Console.ReadLine()?.Trim();
            if (pmChoice == "2")
                sale.PaymentMethod = "GCash";
            else if (pmChoice == "3")
                sale.PaymentMethod = "Maya";
            else
                sale.PaymentMethod = "Cash";

            // Handling payment
            if (sale.PaymentMethod == "Cash")
            {
                Console.Write("Enter cash amount: ");
                if (!double.TryParse(Console.ReadLine(), out double cash)) cash = 0;

                while (cash < sale.GrandTotal)
                {
                    Console.WriteLine($"Insufficient cash. Grand Total is PHP {sale.GrandTotal}. Enter again: ");
                    if (!double.TryParse(Console.ReadLine(), out cash)) cash = 0;
                }

                sale.Cash = cash;
                sale.Change = Math.Round(sale.Cash - sale.GrandTotal, 2);
            }
            else
            {
                // GCash / Maya: treat as exact paid (no change)
                sale.Cash = sale.GrandTotal;
                sale.Change = 0;
            }

            // Save sale, update items file and produce receipt
            saleManager.RecordSale(sale);
            fileManager.SaveItemsToFile(itemManager.GetItems());
            saleManager.ShowReceiptOnScreen(sale);
            saleManager.GenerateReceipt(sale, fileManager.GetFolderPath());

            Console.WriteLine("Transaction complete!");
        }
    }
}
