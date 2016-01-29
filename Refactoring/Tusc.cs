using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactoring
{
    public class Tusc
    {
        public static void Start(List<User> users, List<Product> products)
        {
            OpenMessage();

            bool loggedIn = false;
            while (!loggedIn)
            {
                string name = EnterUserName();

                bool validUser = false;
                if (NameNotEmpty(name))
                {
                    validUser = CheckUserExists(users, name, validUser);

                    if (validUser)
                    {
                        string password = EnterPassword();

                        bool validPassword = ValidatePassword(users, name, password);

                        if (validPassword == true)
                        {
                            loggedIn = true;
                            WelcomeMessage(name);

                            double balance = ShowBalance(users, name, password);

                            UserPurchase(users, products, name, password, ref balance);
                        }
                        else
                        {
                            InvalidPassword();
                        }
                    }
                    else
                    {
                        InvalidUser();
                    }
                }
                else
                {
                    return;
                }
                
            }
            CloseConsole();
        }

        private static void UserPurchase(List<User> users, List<Product> products, string name, string password, ref double balance)
        {
            while (true)
            {
                PrintProductList(products);

                string answer;
                int num = UserProductSelection();

                // Check if user entered number that equals product count
                if (num == 7)
                {
                    UpdateBalance(users, products, name, password, balance);
                    return;
                }
                else
                {
                    ProductSelected(products, ref balance, num);
                }
            }
        }

        private static void ProductSelected(List<Product> products, ref double balance, int num)
        {
            string answer;
            Console.WriteLine();
            Console.WriteLine("You want to buy: " + products[num].Name);
            Console.WriteLine("Your balance is " + balance.ToString("C"));

            int quantity;
            UserQuantitySelection(out answer, out quantity);

            if (!HasEnoughMoney(products, balance, num, quantity) || !HasEnoughQuantity(products, num, quantity))
            {
                return;
            }

            if (quantity > 0)
            {
                balance = SuccessfulPurchase(products, balance, num, quantity);
            }
            else
            {
                NothingPurchased();
            }
        }

        private static void UserQuantitySelection(out string answer, out int quantity)
        {
            Console.WriteLine("Enter amount to purchase:");
            answer = Console.ReadLine();
            quantity = Convert.ToInt32(answer);
        }

        private static bool NameNotEmpty(string name)
        {
            return !string.IsNullOrEmpty(name);
        }

        private static void OpenMessage()
        {
            Console.WriteLine("Welcome to TUSC");
            Console.WriteLine("---------------");
        }

        private static double SuccessfulPurchase(List<Product> products, double balance, int num, int quantity)
        {
            balance = balance - products[num].Price * quantity;

            products[num].Qty = products[num].Qty - quantity;

            PrintPurchase(products, balance, num, quantity);
            return balance;
        }

        private static void PrintPurchase(List<Product> products, double balance, int num, int quantity)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("You bought " + quantity + " " + products[num].Name);
            Console.WriteLine("Your new balance is " + balance.ToString("C"));
            Console.ResetColor();
        }

        private static void NothingPurchased()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("Purchase cancelled");
            Console.ResetColor();
        }

        private static bool HasEnoughQuantity(List<Product> products, int num, int quantity)
        {
            bool validPurchase = true;
            if (products[num].Qty <= quantity)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("Sorry, " + products[num].Name + " is out of stock");
                Console.ResetColor();
                validPurchase = false;
            }
            return validPurchase;
        }

        private static bool HasEnoughMoney(List<Product> products, double balance, int num, int quantity)
        {
            bool validPurchase = true;
            if (balance - products[num].Price * quantity < 0)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("You do not have enough money to buy that.");
                Console.ResetColor();
                validPurchase = false;
            }
            return validPurchase;
        }

        private static void CloseConsole()
        {
            Console.WriteLine();
            Console.WriteLine("Press Enter key to exit");
            Console.ReadLine();
        }

        private static void InvalidUser()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("You entered an invalid user.");
            Console.ResetColor();
        }

        private static void InvalidPassword()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("You entered an invalid password.");
            Console.ResetColor();
        }

        private static void UpdateBalance(List<User> users, List<Product> products, string name, string password, double balance)
        {
            SetUserBalance(users, name, password, balance);

            // Write out new balance
            WriteNewBalance(users);

            // Write out new quantities
            WriteNewQuantities(products);
        }

        private static void SetUserBalance(List<User> users, string name, string password, double balance)
        {
            foreach (var user in users)
            {
                // Check that name and password match
                if (user.Name == name && user.Pwd == password)
                {
                    user.Bal = balance;
                }
            }
        }

        private static void WriteNewQuantities(List<Product> products)
        {
            string quantities = JsonConvert.SerializeObject(products, Formatting.Indented);
            File.WriteAllText(@"Data/Products.json", quantities);
        }

        private static void WriteNewBalance(List<User> users)
        {
            string balances = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(@"Data/Users.json", balances);
        }

        private static void WelcomeMessage(string name)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Login successful! Welcome " + name + "!");
            Console.ResetColor();
        }

        private static int UserProductSelection()
        {
            String answer;
            int num;
            Console.WriteLine("Enter a number:");
            answer = Console.ReadLine();
            num = Convert.ToInt32(answer);
            num = num - 1; // line up product numbering with products list
            return num;
        }

        private static void PrintProductList(List<Product> products)
        {
            Console.WriteLine();
            Console.WriteLine("What would you like to buy?");
            for (int i = 0; i < 7; i++)
            {
                Product product = products[i];
                Console.WriteLine(i + 1 + ": " + product.Name + " (" + product.Price.ToString("C") + ")");
            }
            Console.WriteLine(products.Count + 1 + ": Exit");
        }

        private static double ShowBalance(List<User> users, string name, string password)
        {
            double balance = 0;
            for (int i = 0; i < 5; i++)
            {
                User user = users[i];

                if (user.Name == name && user.Pwd == password)
                {
                    balance = user.Bal;

                    // Show balance 
                    Console.WriteLine();
                    Console.WriteLine("Your balance is " + user.Bal.ToString("C"));
                }
            }
            return balance;
        }

        private static bool ValidatePassword(List<User> users, string name, string password)
        {
            bool validPassword = false;
            for (int i = 0; i < 5; i++)
            {
                User user = users[i];

                if (user.Name == name && user.Pwd == password)
                {
                    validPassword = true;
                }
            }
            return validPassword;
        }

        private static string EnterPassword()
        {
            Console.WriteLine("Enter Password:");
            string password = Console.ReadLine();
            return password;
        }

        private static bool CheckUserExists(List<User> users, string name, bool validUser)
        {
            for (int i = 0; i < 5; i++)
            {
                User user = users[i];
                if (user.Name == name)
                {
                    validUser = true;
                }
            }
            return validUser;
        }

        private static string EnterUserName()
        {
            Console.WriteLine();
            Console.WriteLine("Enter Username:");
            string name = Console.ReadLine();
            return name;
        }
    }
}
