using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/*

Explaination: A battle ships program that allows you to play agianst the computer.
Prerequisites: Newtonsoft.Json NuGet package
Changes: 
I have displayed the MISS when the computer takes the shot to the user as I feel as though it looked better and after continuing a game it meant that the user was refreshed with places the computer had already guessed. 
Moreover it removes junk code, as another character table would have to be included just to record the guesses, which then in return is another value to store in the json files.   


*/



namespace Battleships
{
    internal class Program
    {
        static void Main()
        {

            int opt = Menu(); // receive users option from menu
            switch (opt) // switch for user opt
            {
                case 1:
                    PlayGame(); // call play game function
                    break; // break from switch
                case 2:
                    var data = LoadJson(); // load the json data

                    // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct deconstructing tuples can be found here

                    char[,] playerMap = data.Item1; // declare player map
                    char[,] computerMap = data.Item2; // declare computer map
                    char[,] displayMap = data.Item3; // dedclare player map

                    int playerGuessCount = data.Item4; // declare player guess count
                    int playerHitCount = data.Item5; // declare player hit count
                    int computerGuessCount = data.Item6; // declare computer guess count
                    int computerHitCount = data.Item7; // declare computer hit count

                    if (playerHitCount >= 5 || computerHitCount >= 5) // check if game has been completed already
                    { 
                        Console.WriteLine("This game has already been completed. Start a new game"); // output error message 
                        Main(); // call main function again
                    }
                    GameRunning(playerMap, displayMap, computerMap, playerGuessCount, playerHitCount, computerGuessCount, computerHitCount); // call game running function
                    break; // break from switch
                case 3:
                    DisplayInsutrctions();
                    break; // break from switch
                case 4: // opt 4
                    Console.WriteLine("Au revoir"); // leave nice message after someone didn't want to play my game :(
                    Task.Delay(2000).Wait(); // wait two seconds
                    Environment.Exit(0); // exit the game
                    break; // break from switch
            }
        }

        static void OutputMap(char[,] map)
        {
            for (int y = 0; y < map.GetLength(0); y++) // iterate through each column
            {
                for (int x = 0; x < map.GetLength(1); x++) // iterate through each row in that column
                {
                    Console.Write(map[x, y]); // output each character
                }
                Console.WriteLine(); // after each row output write a new line
            }
        }

        static void DisplayInsutrctions()
        {
            string instructions = @"
Battle boats is a turn based strategy game where players eliminate their opponents fleet of boats by ‘firing’ at a location on a grid in an attempt to sink them. The first player to sink all of their opponents’ battle boats is declared the winner.
Each player has two eight by eight grids. One grid is used for their own battle boats and the other is used to record any hits or misses placed on their opponents. At the beginning of the game, players decide where they wish to place their fleet of five battle boats.
During game play, players take it in turns to fire at a location on their opponent’s board. They do this by stating the coordinates for their target. If a player hits their opponent's boat then this is recorded as a hit. If they miss then this is recorded as a miss. The game ends when a player's fleet of boats have been sunk. The winner is the player with boats remaining at the end of the game."; // declare instructions

            Console.WriteLine("\n\n" + instructions + "\n\n"); // output instructions
            Main(); // call main function again
        }
        static int Menu()
        {
            int opt = 0; // declare opt
            Console.WriteLine("Battle Ships menu: \n1. New Game\n2. Resume A Game\n3. Diplay Insructions\n4. Quit Game"); // output menu message
            try
            {
                opt = Convert.ToInt32(Console.ReadLine()); // try and convert the users option to an int
                if (opt < 1 || opt > 4) // check if the option is within the correct menu options
                {
                    Console.Clear(); // if not clear the console
                    Console.WriteLine("Your option wasn't a valid menu option. Please try again:  "); // displayer error message
                    Menu(); // call menu function again
                }

            }
            catch (Exception) // catch the exception if opt is not an int
            {
                Console.Clear(); // clear the console
                Console.WriteLine("Your input is invalid. Please try again: "); // display error message
                Menu(); // call menu function again
            }
            return opt; // return the option the user picked
        }

        static char[,] GenComputerMap()
        {
            char[,] ComputerMap = new char[8, 8]{
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            }; // declare empty computer map

            Random random = new Random(); // initalise new random seed
            for (int boatCount = 0; boatCount < 5;) // iterate until five boats are chosen
            {
                int xValue = random.Next(0, 8); // generate xValue
                int yValue = random.Next(0, 8); // generate yValue
                if (ComputerMap[xValue, yValue] != '*') // check if there isn't a boat already in the space
                {
                    ComputerMap[xValue, yValue] = '*'; // if not set the boat on the player map
                    boatCount++; // add one to the boat count
                }
            }
            return ComputerMap; // return newly generated computer map

        }
        static void PlayGame()
        {
            char[,] displayMap = new char[8, 8]{
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            }; // declare empty display map

            char[,] playerMap = new char[8, 8]{
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            { '.','.','.','.','.','.','.','.'},
            }; // declare empty player map
            int boatCount = 0; // declare boat count
            while (boatCount < 5) // loop while the boat count is less than five
            {
                Console.WriteLine("Enter the X value of boat {0}: ", boatCount + 1); // Output prompt message
                int xValue = 10; // declare xValue
                try
                {
                    xValue = Convert.ToInt32(Console.ReadLine()); // try to convert xValue to int
                }
                catch (Exception) // catch exception if xValue is not an int
                {
                    Console.WriteLine("Invalid data type."); // output error message
                }
                if (xValue < 1 || xValue > 8) // check if x coordinate is within the correct bounds
                {
                    Console.WriteLine("Please ensure your X value is within the correct coordinates."); // output error message

                }
                else // x coordinate was valid
                {
                    Console.WriteLine("Enter the Y value of boat {0}: ", boatCount + 1); // Output prompt message
                    int yValue = 10; // declare yValue
                    try
                    {
                       yValue = Convert.ToInt32(Console.ReadLine()); // try to convert yValue to int

                    }
                    catch (Exception) // catch exception if yValue is not an int
                    {
                        Console.WriteLine("Invalid data type"); // displayer error message 
                    }
                    if (yValue < 1 || yValue > 8) // check if users coordinates are within the right range 
                    {
                        Console.WriteLine("Please ensure your Y value is within the correct coordinates."); // displayer error message
                    }
                    else if (playerMap[xValue - 1, 8 - yValue] == '*') // check if a user has already placed the boat in that position
                    {
                        Console.WriteLine("You can't place a boat in the same position."); // displayer error message
                    }
                    else // valid data
                    {
                        playerMap[xValue - 1, 8 - yValue] = '*'; // set player map boat
                        boatCount++; // add one to the boat count
                        Console.Clear(); // clear console
                        OutputMap(playerMap); // output the new player map
                        Console.WriteLine("\n\n"); // output two blank lines


                    }


                }


            }
            char[,] computerMap = GenComputerMap(); // generate the computer map
            Console.Clear(); // clear console
            Console.WriteLine("Game Starting In 5 Seconds."); // output game starting message
            Task.Delay(5000).Wait(); // wait five seconds
            GameRunning(playerMap, displayMap, computerMap, 0, 0, 0, 0); // call the game running function (game now begins)

        }


        static void GameRunning(char[,] playerMap, char[,] displayMap, char[,] computerMap, int playerGuessCount, int playerHitCount, int computerGuessCount, int computerHitCount)
        {
            Console.Clear(); // clear the console
            Console.WriteLine("<-----| YOUR MAP: |----->\n\n"); // present player map title
            OutputMap(playerMap); // present player map
            Console.WriteLine("\n\n<-----| DISPLAY MAP: |----->\n\n"); // present display map tittle
            OutputMap(displayMap); // present display map
            Console.Write("\n\n"); // two blank lines
            while ((playerHitCount < 5) || (computerHitCount < 5)) // loop while the player hit count is less than five or computer hit count is less than five
            {
                (playerHitCount, displayMap) = PlayerGuess(displayMap, computerMap, playerHitCount); // player takes guess
                OutputMap(displayMap); // output the new display map
                playerGuessCount++; // add one to the player guess count
                if (playerHitCount == 5) // check if the player has won
                {
                    Console.WriteLine("You Won in {0} guesses", playerGuessCount); // displayer win message
                    WriteJson(playerMap, displayMap, computerMap, playerGuessCount, playerHitCount, computerGuessCount, computerHitCount); // write the game to json the file
                    break; // break from while loop
                }
                Console.WriteLine("\n\nComputer is guessing: \n\n"); // output computer guess message
                Task.Delay(2000).Wait(); // wait two seconds

                (computerHitCount, playerMap) = ComputerGuess(playerMap, computerHitCount); // computer takes guess
                computerGuessCount++; // add one to the computer guess count
                if (computerHitCount == 5) // check if the computer has won
                {
                    Console.WriteLine("The computer won in {0} guesses", computerGuessCount); // displayer win message
                    WriteJson(playerMap, displayMap, computerMap, playerGuessCount, playerHitCount, computerGuessCount, computerHitCount); // write the game to the json file
                }
                OutputMap(playerMap); // output player map
                WriteJson(playerMap, displayMap, computerMap, playerGuessCount, playerHitCount, computerGuessCount, computerHitCount); // write the current data to the json file

            }




        }
        static (int, char[,]) ComputerGuess(char[,] playerMap, int computerHitCount)
        {
            Random random = new Random(); // initalise new random seed
            int xValue = random.Next(0, 8); // generate random x coordinate
            int yValue = random.Next(0, 8); // generate random y coordinate
            if (playerMap[xValue, yValue] == '*') // check if the computers coordinates are a hit on the players map
            {
                computerHitCount++; // if so add one to computer hit count
                playerMap[xValue, yValue] = 'H'; // displayer a H on players map
                Console.WriteLine("Computer Guessed: ({0},{1}) and hit! OH NO!", xValue + 1, 8 - yValue); // output hit message
            }
            else if (playerMap[xValue, yValue] == '.') // else if the computer missed
            {
                Console.WriteLine("Computer Guessed: ({0},{1}) and missed. Thank god!", xValue + 1, 8 - yValue); // output miss message
                playerMap[xValue, yValue] = 'M'; // display a M on players map
            }
            else // else the user has already guessed the space
            {
                ComputerGuess(playerMap, computerHitCount); // call the computer guess function again to regenerate the new coordinates
            }


            return (computerHitCount, playerMap);  // return the new computer hit count and player map
        }

        static (int, char[,]) PlayerGuess(char[,] displayMap, char[,] computerMap, int playerHitCount)
        {
            Console.WriteLine("\n\nEnter the X value of your guess: "); // prompt user for their x coordinate
            int xValue = 10; // declare xValue
            try
            {
                xValue = Convert.ToInt32(Console.ReadLine()); // try and convert xValue to int

            }
            catch (Exception) // catch the try exception incase on non int value
            {
                Console.Write("Invalid data type"); // output error message
            }
            if (xValue < 1 || xValue > 8) // check if x coordinate are within the correct bounds
            {
                Console.WriteLine("Please ensure your X value is within the correct coordinates."); // output error message
                PlayerGuess(displayMap, computerMap, playerHitCount); // call the player guess function again to retake their go

            }
            else // else x coordinate must be valid
            {
                Console.WriteLine("\n\nEnter the Y value of your guess: "); // prompt the user for their y coordinate
                int yValue = 10; // declare yValue
                try
                {
                   yValue = Convert.ToInt32(Console.ReadLine()); // try and convert yValue to int
                }
                catch (Exception) // catch the try exception in case of a non int value
                {
                    Console.WriteLine("Invalid data type"); // output error message
                    PlayerGuess(displayMap, computerMap, playerHitCount); // call player guess function as they need to retake their go
                }
                if (yValue < 1 || yValue > 8) // check if y coordinates are within the correct bounds
                {
                    Console.WriteLine("Please ensure your Y value is within the correct coordinates."); // output error message
                    PlayerGuess(displayMap, computerMap, playerHitCount); // call the player guess function again to retake their go
                }
                else if ((displayMap[xValue-1, 8-yValue] == 'M') || (displayMap[xValue - 1, 8 - yValue] == 'H')) // check if user has shot in the given place already
                {
                    Console.WriteLine("You can't fire in the same position."); // output error message
                    PlayerGuess(displayMap, computerMap, playerHitCount); // call the player guess function as they need to retake their go
                }
                else if (computerMap[xValue - 1, 8 - yValue] == '.') // check if there is a boat on in the coordinates
                {
                    Console.Clear(); // clear console
                    Console.WriteLine("You Guessed: ({0},{1}) and missed.", xValue, yValue); // output miss message
                    displayMap[xValue - 1, 8 - yValue] = 'M'; // set the coordinate on the grid to M value

                }
                else // else must be a hit
                {
                    Console.Clear(); // clear console
                    Console.WriteLine("You Guessed: ({0},{1}) and hit! Nice Shot!", xValue, yValue); // output hit message
                    displayMap[xValue - 1, 8 - yValue] = 'H'; // set the coordinate on the grid to H value
                    playerHitCount++; // add one to player hit count
                }
                
            }
            return (playerHitCount, displayMap); // return the new player hit count and display map
        }

        static dynamic LoadJson()
        {
            if (!File.Exists(@"game.json")) // check if the file exists and the user hasn't tried to load a game without playing a game first
            {
                Console.WriteLine("Couldn't find a saved file."); // means they haven't played a game yet
                Main(); // call main function again
            }
            using (StreamReader r = new StreamReader(@"game.json")) // read game.json file
            {
                string json = r.ReadToEnd(); // read until the end of the file
                dynamic data = JsonConvert.DeserializeObject(json); // deserialise the json object
                
                string playerMap = data.playerMap; // declare player map
                string computerMap = data.computerMap; // declare computer map
                string displayMap = data.displayMap; // declare display map
                int playerGuessCount = data.playerGuessCount; // declare player guess count
                int playerHitCount = data.playerHitCount; // declare player hit count
                int computerGuessCount = data.computerGuessCount; // declare computer guess count
                int computerHitCount = data.computerHitCount; // declare computer hit count
            

                char[,] desPlayerMap = JsonConvert.DeserializeObject<char[,]>(playerMap); // deserialise player map to 2D char arary
                char[,] desComputerMap = JsonConvert.DeserializeObject<char[,]>(computerMap); // deserialise computer map to 2D char array
                char[,] desDisplayMap = JsonConvert.DeserializeObject<char[,]>(displayMap); // deserialise player map to 2D char arary

                return (desPlayerMap, desComputerMap, desDisplayMap, playerGuessCount, playerHitCount, computerGuessCount, computerHitCount); // return the new deserialised maps

            }
        }


        static void WriteJson(char[,] playerMap, char[,] displayMap, char[,] computerMap, int playerGuessCount, int playerHitCount, int computerGuessCount, int computerHitCount)
        {

            JObject game = new JObject( // creates a new json object
                new JProperty("playerMap", JsonConvert.SerializeObject(playerMap)), // new json property containing the player map (serialise it make it a string)
                new JProperty("displayMap", JsonConvert.SerializeObject(displayMap)), // new json property containing the display map (serialise it make it a string)
                new JProperty("computerMap", JsonConvert.SerializeObject(computerMap)), // new json property containing the computer map(serialise it make it a string)
                new JProperty("playerGuessCount", playerGuessCount), // new json property containing player guess count
                new JProperty("playerHitCount", playerHitCount), // new json property containing player hit count
                new JProperty("computerGuessCount", computerGuessCount), // new json property containing computer guess count
                new JProperty("computerHitCount", computerHitCount) // new json property containing computer hit count
                );

            File.WriteAllText(@"game.json", game.ToString()); // write the json object to the file
        }

    }
}
