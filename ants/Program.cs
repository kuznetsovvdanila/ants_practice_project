using System;
using System.Collections.Generic;

namespace ants
{
    // 12
    internal class Program
    {
        class Unit : ICloneable
        {
            public string Name = "Undefined";
            public int Health = 0;
            public int Defence = 0;
            public int Damage = 0;
            public bool IsAlive = true;
            
            public Colony ItsColony;
            public List<string> Mods = new List<string>();
            
            public object Clone()
            {
                return MemberwiseClone();
            }
        }

        class AntQueen : Unit
        {
            public AntQueen Mother;
            public int[] NewQueenGrowth = new int[2];
            public int[] NewQueenMax = new int[2];
            public bool CanMakeChildren = true;
            public bool Adult = true;
            public int Age = 0;
            public List<AntQueen> SmallQueens = new List<AntQueen>();
        }
        class AntWarrior : Unit
        {
            public int AttackedTargetsAtOnce = 0;
            public int BitesWhileAttack = 0;
            public bool InstantKill = false;
            public bool AttacksTeammates = false;
            public int HowManyTimesAttacked = 0;

            public void LetMeIntroduceMyself()
            {
                Console.WriteLine($"Тип: {Name}\n --- Параметры: здоровье({Health}), защита({Defence}), урон({Damage})");
                if (Mods.Count > 0)
                {
                    Console.Write(" --- Модификаторы: ");
                    Mods.ForEach(delegate(string s) { Console.Write($"{s}, "); });
                    Console.Write("\n");
                }
                Console.WriteLine($" --- Королева: {ItsColony.Queen.Name}");
                Console.WriteLine();
            }
            
            public void AttackWarrior(AntWarrior enemyWarrior)
            {
                if (InstantKill)
                {
                    if (enemyWarrior.InstantKill && enemyWarrior.HowManyTimesAttacked < enemyWarrior.AttackedTargetsAtOnce) IsAlive = false;
                    else Health -= enemyWarrior.Defence;
                    enemyWarrior.IsAlive = false;
                }
                else
                {
                    if (enemyWarrior.InstantKill && enemyWarrior.HowManyTimesAttacked < enemyWarrior.AttackedTargetsAtOnce) IsAlive = false;
                    else
                    {
                        if (enemyWarrior.HowManyTimesAttacked < enemyWarrior.AttackedTargetsAtOnce)
                        {
                            Health -= enemyWarrior.Damage * enemyWarrior.BitesWhileAttack;
                            enemyWarrior.Health -= Defence * enemyWarrior.BitesWhileAttack;
                        }
                        Health -= enemyWarrior.Defence * BitesWhileAttack;
                        enemyWarrior.Health -= Damage * BitesWhileAttack;
                    }
                }
                HowManyTimesAttacked++;
                if (enemyWarrior.HowManyTimesAttacked < enemyWarrior.AttackedTargetsAtOnce) enemyWarrior.HowManyTimesAttacked++;

                if (Health <= 0)
                {
                    Health = 0;
                    IsAlive = false;
                }
                
                if (enemyWarrior.Health <= 0)
                {
                    enemyWarrior.Health = 0;
                    enemyWarrior.IsAlive = false;
                }
            }

            public void AttackSpecial(NotAntSpecial enemySpecial)
            {
                if (enemySpecial.Immortal)
                {
                    Health -= (enemySpecial.Damage * enemySpecial.BitesWhileAttack + enemySpecial.Defence * BitesWhileAttack);
                }
                else
                {
                    if (InstantKill)
                    {
                        enemySpecial.IsAlive = false;
                        Health -= enemySpecial.Defence;
                    }
                    else
                    {
                        enemySpecial.Health -= (Damage * BitesWhileAttack + Defence * enemySpecial.BitesWhileAttack);
                        Health -= (enemySpecial.Damage * enemySpecial.BitesWhileAttack + enemySpecial.Defence * BitesWhileAttack);
                    }
                }
                if (Health <= 0) IsAlive = false;
                if (enemySpecial.Health <= 0)
                {
                    enemySpecial.IsAlive = false;
                    if (enemySpecial.KillsHisMurderer) IsAlive = false;
                }
                HowManyTimesAttacked++;
                if (enemySpecial.CanAttack && enemySpecial.HowManyTimesAttacked < enemySpecial.AttackedTargetsAtOnce) enemySpecial.HowManyTimesAttacked++;
            }
            
            public void AttackWorker(AntWorker enemyWorker)
            {
                if (enemyWorker.IsAlive)
                {
                    Health -= enemyWorker.Defence;
                }
                enemyWorker.Health = 0;
                enemyWorker.IsAlive = false;
                HowManyTimesAttacked++;
            }
        }

        class AntWorker : Unit
        {
            public int ResourcesAtOnce = 0;
            public List<string> Resources = new List<string>();
            public bool CanTakeInvisibleResources = false;

            // if a worker can take all of the possible resources at once
            // or if it can choose from resources presented in certain stack 
            public bool ResourceAndResource = false;

            public void LetMeIntroduceMyself()
            {
                Console.WriteLine($"Тип: {Name}\n --- Параметры: здоровье({Health}), защита({Defence})");
                if (Mods.Count > 0)
                {
                    Console.Write(" --- Модификаторы: ");
                    Mods.ForEach(delegate(string s) { Console.Write($"{s}, "); });
                    Console.Write("\n");
                }
                Console.WriteLine($" --- Королева: {ItsColony.Queen.Name}");
                Console.WriteLine();
            }
            
            public Stack TakeResources(Stack stack)
            {
                Random rnd = new Random();
                int resourceNumber;
                if (ResourceAndResource)
                {
                    var counter = 0;
                    List<string> resourcesTriedToTake = new List<string>();
                    bool flag = true;

                    foreach (var resource in Resources)
                    {
                        if (!(stack.Resources[resource] > 0) && !CanTakeInvisibleResources)
                        {
                            return stack;
                        }
                    }

                    foreach (var resource in Resources)
                    {
                        if (stack.Resources[resource] > 0)
                        {
                            ItsColony.Resources[resource]++;
                            stack.Resources[resource]--;
                            counter++;
                        }
                    }

                    while (counter < ResourcesAtOnce && flag)
                    {
                        resourceNumber = rnd.Next(Resources.Count);
                        if (!resourcesTriedToTake.Contains(Resources[resourceNumber]))
                        {
                            resourcesTriedToTake.Add(Resources[resourceNumber]);
                        }
                        if (stack.Resources[Resources[resourceNumber]] > 0 || CanTakeInvisibleResources)
                        {
                            ItsColony.Resources[Resources[resourceNumber]]++;
                            stack.Resources[Resources[resourceNumber]]--;
                            counter++;
                        }
                        for (var i = 0; i < Resources.Count; i++)
                        {
                            if (!resourcesTriedToTake.Contains(Resources[i]))
                            {
                                break;
                            }

                            if (i == Resources.Count - 1)
                            {
                                flag = false;
                            }
                        }
                    }
                }
                else
                {
                    var counter = 0;
                    List<string> resourcesTriedToTake = new List<string>();
                    bool flag = true;
                    
                    while (counter < ResourcesAtOnce && flag)
                    {
                        resourceNumber = rnd.Next(Resources.Count);
                        if (!resourcesTriedToTake.Contains(Resources[resourceNumber]))
                        {
                            resourcesTriedToTake.Add(Resources[resourceNumber]);
                        }
                        if (stack.Resources[Resources[resourceNumber]] > 0 || CanTakeInvisibleResources)
                        {
                            ItsColony.Resources[Resources[resourceNumber]]++;
                            stack.Resources[Resources[resourceNumber]]--;
                            counter++;
                        }

                        for (var i = 0; i < Resources.Count; i++)
                        {
                            if (!resourcesTriedToTake.Contains(Resources[i]))
                            {
                                break;
                            }

                            if (i == Resources.Count - 1)
                            {
                                flag = false;
                            }
                        }
                    }
                }
                
                return stack;
            }
        }

        class NotAntSpecial : Unit
        {
            public bool CanAttack = false;
            public int AttackedTargetsAtOnce = 0;
            public int BitesWhileAttack = 0;
            public bool Immortal = false;
            public bool KillsHisMurderer = false;
            public int HowManyTimesAttacked = 0;
            
            public bool CanTakeResources = false;
            public int ResourcesAtOnce = 0;
            public bool ResourceAndResource = false;
            public bool CanTakeInvisibleResources = false;
            public List<string> Resources = new List<string>();

            public void LetMeIntroduceMyself()
            {
                Console.WriteLine($"Тип: {Name}\n --- Параметры: здоровье({Health}), защита({Defence}), урон({Damage})");
                if (Mods.Count > 0)
                {
                    Console.Write(" --- Модификаторы: ");
                    Mods.ForEach(delegate(string s) { Console.Write($"{s}, "); });
                    Console.Write("\n");
                }
                Console.WriteLine($" --- Королева: {ItsColony.Queen.Name}");
                Console.WriteLine();
            }
            
            public Stack TakeResources(Stack stack)
            {
                Random rnd = new Random();
                int resourceNumber;
                if (ResourceAndResource)
                {
                    var counter = 0;
                    List<string> resourcesTriedToTake = new List<string>();
                    bool flag = true;

                    foreach (var resource in Resources)
                    {
                        if (!(stack.Resources[resource] > 0) && !CanTakeInvisibleResources)
                        {
                            return stack;
                        }
                    }

                    foreach (var resource in Resources)
                    {
                        if (stack.Resources[resource] > 0)
                        {
                            ItsColony.Resources[resource]++;
                            stack.Resources[resource]--;
                            counter++;
                        }
                    }

                    while (counter < ResourcesAtOnce && flag)
                    {
                        resourceNumber = rnd.Next(Resources.Count);
                        if (!resourcesTriedToTake.Contains(Resources[resourceNumber]))
                        {
                            resourcesTriedToTake.Add(Resources[resourceNumber]);
                        }
                        if (stack.Resources[Resources[resourceNumber]] > 0 || CanTakeInvisibleResources)
                        {
                            ItsColony.Resources[Resources[resourceNumber]]++;
                            stack.Resources[Resources[resourceNumber]]--;
                            counter++;
                        }
                        for (var i = 0; i < Resources.Count; i++)
                        {
                            if (!resourcesTriedToTake.Contains(Resources[i]))
                            {
                                break;
                            }

                            if (i == Resources.Count - 1)
                            {
                                flag = false;
                            }
                        }
                    }
                    Console.WriteLine("-----");
                }
                else
                {
                    var counter = 0;
                    List<string> resourcesTriedToTake = new List<string>();
                    bool flag = true;
                    
                    while (counter < ResourcesAtOnce && flag)
                    {
                        resourceNumber = rnd.Next(Resources.Count);
                        if (!resourcesTriedToTake.Contains(Resources[resourceNumber]))
                        {
                            resourcesTriedToTake.Add(Resources[resourceNumber]);
                        }
                        if (stack.Resources[Resources[resourceNumber]] > 0 || CanTakeInvisibleResources)
                        {
                            ItsColony.Resources[Resources[resourceNumber]]++;
                            stack.Resources[Resources[resourceNumber]]--;
                            counter++;
                        }

                        for (var i = 0; i < Resources.Count; i++)
                        {
                            if (!resourcesTriedToTake.Contains(Resources[i]))
                            {
                                break;
                            }

                            if (i == Resources.Count - 1)
                            {
                                flag = false;
                            }
                        }
                    }
                }
                
                return stack;
            }

            public NotAntSpecial AttackSpecial(NotAntSpecial enemySpecial)
            {
                if (enemySpecial.Immortal)
                {
                    if (!Immortal)
                    {
                        if (CanAttack) Health -= enemySpecial.Defence * BitesWhileAttack;
                        if (enemySpecial.CanAttack && enemySpecial.HowManyTimesAttacked < enemySpecial.AttackedTargetsAtOnce) Health -= enemySpecial.Damage * enemySpecial.BitesWhileAttack;
                        if (Health <= 0) IsAlive = false;
                    }
                }
                else
                {
                    if (Immortal)
                    {
                        if (CanAttack) enemySpecial.Health -= Damage * BitesWhileAttack;
                        if (enemySpecial.Health <= 0) enemySpecial.IsAlive = false;
                    }

                    if (enemySpecial.HowManyTimesAttacked < enemySpecial.AttackedTargetsAtOnce) Health -= enemySpecial.Damage * enemySpecial.BitesWhileAttack;
                    Health -= enemySpecial.Defence * BitesWhileAttack;
                    if (enemySpecial.HowManyTimesAttacked < enemySpecial.AttackedTargetsAtOnce) enemySpecial.Health -= Defence * enemySpecial.BitesWhileAttack;
                    enemySpecial.Health -= Damage * BitesWhileAttack;
                    if (Health <= 0) IsAlive = false;
                    if (enemySpecial.Health <= 0) enemySpecial.IsAlive = false;
                }

                if (enemySpecial.CanAttack && enemySpecial.HowManyTimesAttacked < enemySpecial.AttackedTargetsAtOnce) enemySpecial.HowManyTimesAttacked++;
                HowManyTimesAttacked++;
                return enemySpecial;
            }
            
            public AntWarrior AttackWarrior(AntWarrior enemyWarrior)
            {
                if (Immortal)
                {
                    if (CanAttack) enemyWarrior.Health -= Defence * enemyWarrior.BitesWhileAttack;
                    if (enemyWarrior.HowManyTimesAttacked < enemyWarrior.AttackedTargetsAtOnce) enemyWarrior.Health -= Damage * BitesWhileAttack;
                    if (enemyWarrior.Health <= 0) IsAlive = false;
                }
                else
                {
                    if (enemyWarrior.InstantKill && enemyWarrior.HowManyTimesAttacked < enemyWarrior.AttackedTargetsAtOnce) IsAlive = false;
                    else
                    {
                        if (CanAttack) enemyWarrior.Health -= Damage * BitesWhileAttack;
                        if (enemyWarrior.HowManyTimesAttacked < enemyWarrior.AttackedTargetsAtOnce)
                        {
                            enemyWarrior.Health -= Defence * enemyWarrior.BitesWhileAttack;
                            Health -= enemyWarrior.Damage * enemyWarrior.BitesWhileAttack;
                        }
                        enemyWarrior.Health -= Damage * BitesWhileAttack;
                        Health -= enemyWarrior.Defence * BitesWhileAttack;
                        if (Health <= 0) IsAlive = false;
                        if (enemyWarrior.Health <= 0) enemyWarrior.IsAlive = false;
                    }
                }

                if (enemyWarrior.HowManyTimesAttacked < enemyWarrior.AttackedTargetsAtOnce) enemyWarrior.HowManyTimesAttacked++;
                HowManyTimesAttacked++;
                return enemyWarrior;
            }

            public AntWorker AttackWorker(AntWorker enemyWorker)
            {
                if (enemyWorker.IsAlive)
                {
                    Health -= enemyWorker.Defence;
                    HowManyTimesAttacked++;
                }
                enemyWorker.Health = 0;
                enemyWorker.IsAlive = false;
                return enemyWorker;
            }
        }

        class Colony : ICloneable
        {
            public int Type;
            public string Name;
            
            // colony's queen
            public AntQueen Queen;
            
            // colony's units
            public List<AntWarrior> Warriors = new List<AntWarrior>();
            public List<AntWorker> Workers = new List<AntWorker>();
            public int MaxWarriorsAmount = 0;
            public int MaxWorkersAmount = 0;
            
            // unit types available for certain colony
            public AntWarrior[] WarriorTypes = new AntWarrior[3];
            public AntWorker[] WorkerTypes = new AntWorker[3];

            // colony's special unit
            public NotAntSpecial Special;
            
            // colony's resources
            public Dictionary<string, int> Resources = new Dictionary<string, int>
            {
                {"Vetochka", 0},
                {"Listik", 0},
                {"Kamushek", 0},
                {"Rosinka", 0}
            };
            
            public int ResourcesCounter()
            {
                int summ = 0;
                foreach (var resource in Resources)
                {
                    summ += resource.Value;
                }
                return summ;
            }
            
            public void AddUnits()
            {
                Random rnd = new Random();
                for (var j = 0; j < MaxWarriorsAmount; j++)
                {
                    Warriors.Add((AntWarrior)WarriorTypes[rnd.Next(3)].Clone());
                    Warriors[j].ItsColony = this;
                }
                for (var j = 0; j < MaxWorkersAmount; j++)
                {
                    Workers.Add((AntWorker)WorkerTypes[rnd.Next(3)].Clone());
                    Workers[j].ItsColony = this;
                }
            }

            // counts all posible units in colony, except the queen and special unit
            public int CountPopulation()
            {
                var population = 0;
                for (var i = 0; i < Warriors.Count; i++)
                {
                    population++;
                }
                for (var i = 0; i < Workers.Count; i++)
                {
                    population++;
                }
                if (Special != null) population++;
                return population;
            }

            public void LetUsIntroduceOurselves()
            {
                Console.WriteLine($"Колония '{Name}'\n --- Королева '{Queen.Name}': здоровье({Queen.Health}), защита({Queen.Defence}), урон({Queen.Damage})\n --- Ресурсы: веточка({Resources["Vetochka"]}), листик({Resources["Listik"]}), камушек({Resources["Kamushek"]}), росинка({Resources["Rosinka"]})");
                
                Console.WriteLine("\nРабочие:\n");
                for (var j = 0; j < Workers.Count; j++)
                {
                    Console.WriteLine($"Тип: {Workers[j].Name}\n --- Параметры: здоровье({Workers[j].Health}), защита({Workers[j].Defence})");
                    if (Workers[j].Mods.Count > 0)
                    {
                        Console.Write(" --- Модификаторы: ");
                        Workers[j].Mods.ForEach(delegate(string s) { Console.Write($"{s}, "); });
                        Console.Write("\n");
                    }
                    Console.WriteLine($" --- Максимальное количество: {Workers.Count}");
                    Console.WriteLine();

                }
                Console.WriteLine("\nВоины:\n");
                for (var j = 0; j < Warriors.Count; j++)
                {
                    Console.WriteLine($"Тип: {Warriors[j].Name}\n --- Параметры: здоровье({Warriors[j].Health}), защита({Warriors[j].Defence}), урон({Warriors[j].Damage})");
                    if (Warriors[j].Mods.Count > 0)
                    {
                        Console.Write(" --- Модификаторы: ");
                        Warriors[j].Mods.ForEach(delegate(string s) { Console.Write($"{s}, "); });
                        Console.Write("\n");
                    }
                    Console.WriteLine($" --- Максимальное количество: {Warriors.Count}");
                    Console.WriteLine();
                }

                if (Special != null)
                {
                    Console.WriteLine("\nСпециальные\n");
                    Console.WriteLine($"Тип: {Special.Name}\n --- Параметры: здоровье({Special.Health}), защита({Special.Defence}), урон({Special.Damage})");
                    if (Special.Mods.Count > 0)
                    {
                        Console.Write(" --- Модификаторы: ");
                        Special.Mods.ForEach(delegate(string s) { Console.Write($"{s}, "); });
                        Console.Write("\n");
                    }
                    Console.WriteLine(" --- Максимальное количество: 1");
                    Console.WriteLine();
                }
            }
            
            // to make another instances of colony
            public object Clone()
            {
                return MemberwiseClone();
            }
        }

        class Stack
        {
            public Dictionary<string, int> Resources = new Dictionary<string, int>();
            public List<AntWarrior> WarriorsList = new List<AntWarrior>();
            public List<AntWorker> WorkersList = new List<AntWorker>();
            public List<NotAntSpecial> SpecialsList = new List<NotAntSpecial>();
            public List<Colony> ColoniesList = new List<Colony>();
            
            public bool IsEmpty()
            {
                int resourcesAmount = 0;
                foreach (var resource in Resources)
                {
                    resourcesAmount += resource.Value;
                }
                return resourcesAmount <= 0;
            }

            // if used, there are at least 1 warrior or special unit
            public void BattleChecker()
            {
                int unitsAmount = WarriorsList.Count + WorkersList.Count + SpecialsList.Count;
                if (unitsAmount > 1)
                {
                    Random rnd = new Random();
                    if (WarriorsList.Count > 0)
                    {
                        // warriors' attacks
                        for (var i = 0; i < WarriorsList.Count; i++)
                        {
                            int unitNumber;
                            for (var j = WarriorsList[i].HowManyTimesAttacked; j < WarriorsList[i].AttackedTargetsAtOnce; j++)
                            {
                                List<int> possibleNumbers = new List<int>();
                                while (WarriorsList[i].IsAlive)
                                {
                                    // initializing random variable
                                    unitNumber = rnd.Next(unitsAmount);
                                    if (!possibleNumbers.Contains(unitNumber))
                                    {
                                        possibleNumbers.Add(unitNumber);
                                    }
                                    // checking possible attacks on warriors
                                    if (unitNumber < WarriorsList.Count)
                                    {
                                        if (WarriorsList[unitNumber].ItsColony.Type != WarriorsList[i].ItsColony.Type)
                                        {
                                            if (!WarriorsList[i].AttacksTeammates)
                                            {
                                                WarriorsList[i].AttackWarrior(WarriorsList[unitNumber]);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (WarriorsList[i].AttacksTeammates && WarriorsList[i] != WarriorsList[unitNumber])
                                            {
                                                WarriorsList[i].AttackWarrior(WarriorsList[unitNumber]);
                                                break;
                                            }
                                        }
                                    }
                                    // checking possible attacks on workers
                                    else if (WorkersList.Count > 0 && unitNumber >= WarriorsList.Count && unitNumber < WarriorsList.Count + WorkersList.Count)
                                    {
                                        if (WorkersList[unitNumber - WarriorsList.Count].ItsColony.Type != WarriorsList[i].ItsColony.Type)
                                        {
                                            if (!WarriorsList[i].AttacksTeammates)
                                            {
                                                WarriorsList[i].AttackWorker(WorkersList[unitNumber - WarriorsList.Count]);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (WarriorsList[i].AttacksTeammates)
                                            {
                                                WarriorsList[i].AttackWorker(WorkersList[unitNumber - WarriorsList.Count]);
                                                break;
                                            }
                                        }
                                    }
                                    // checking possible attacks on specials
                                    else if (SpecialsList.Count > 0 && unitNumber >= WarriorsList.Count + WorkersList.Count)
                                    {
                                        if (SpecialsList[unitNumber - (WarriorsList.Count + WorkersList.Count)].ItsColony.Type != WarriorsList[i].ItsColony.Type)
                                        {
                                            if (!WarriorsList[i].AttacksTeammates)
                                            {
                                                WarriorsList[i].AttackSpecial(SpecialsList[unitNumber - (WarriorsList.Count + WorkersList.Count)]);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (WarriorsList[i].AttacksTeammates)
                                            {
                                                WarriorsList[i].AttackSpecial(SpecialsList[unitNumber - (WarriorsList.Count + WorkersList.Count)]);
                                                break;
                                            }
                                        }
                                    }

                                    if (possibleNumbers.Count == unitsAmount)
                                    {
                                        break;
                                    }
                                }
                            }

                            WarriorsList[i].HowManyTimesAttacked = 0;
                        }
                    }

                    if (SpecialsList.Count > 0)
                    {
                        for (var i = 0; i < SpecialsList.Count; i++)
                        {
                            if (SpecialsList[i].CanAttack)
                            {
                                int unitNumber;
                                for (var j = 0; j < SpecialsList[i].AttackedTargetsAtOnce - SpecialsList[i].HowManyTimesAttacked; j++)
                                {
                                    while (SpecialsList[i].IsAlive)
                                    {
                                        // initializing random variable
                                        unitNumber = rnd.Next(unitsAmount);
                                        // checking possible attacks on warriors
                                        if (unitNumber < WarriorsList.Count)
                                        {
                                            if (WarriorsList[unitNumber].ItsColony.Type != SpecialsList[i].ItsColony.Type)
                                            {
                                                WarriorsList[unitNumber] = SpecialsList[i].AttackWarrior(WarriorsList[unitNumber]);
                                                break;
                                            }
                                        }
                                        // checking possible attacks on workers
                                        else if (WorkersList.Count > 0 && unitNumber >= WarriorsList.Count && unitNumber < WarriorsList.Count + WorkersList.Count)
                                        {
                                            if (WorkersList[unitNumber - WarriorsList.Count].ItsColony.Type != SpecialsList[i].ItsColony.Type)
                                            {
                                                WorkersList[unitNumber - WarriorsList.Count] = SpecialsList[i].AttackWorker(WorkersList[unitNumber - WarriorsList.Count]);
                                                break;
                                            }
                                        }
                                        // checking possible attacks on specials
                                        else if (SpecialsList.Count > 1 && unitNumber >= WarriorsList.Count + WorkersList.Count && SpecialsList[unitNumber - (WarriorsList.Count + WorkersList.Count)].ItsColony.Type != SpecialsList[i].ItsColony.Type)
                                        {
                                            if (SpecialsList[unitNumber - (WarriorsList.Count + WorkersList.Count)] != SpecialsList[i])
                                            {
                                                SpecialsList[unitNumber - (WarriorsList.Count + WorkersList.Count)] = SpecialsList[i].AttackSpecial(SpecialsList[unitNumber - (WarriorsList.Count + WorkersList.Count)]);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            SpecialsList[i].HowManyTimesAttacked = 0;
                        }
                    }
                }
            }
        }
        
        public static void Main(string[] args)
        {
            // stacks array
            List<Stack> stacks = new List<Stack>()
            {
                new Stack { Resources = { {"Vetochka", 49}, {"Listik", 38}, {"Kamushek", 17}, {"Rosinka", 37} } },
                new Stack { Resources = { {"Vetochka", 42}, {"Listik", 33}, {"Kamushek", 11}, {"Rosinka", 40} } },
                new Stack { Resources = { {"Vetochka", 37}, {"Listik", 10}, {"Kamushek", 44}, {"Rosinka", 0} } },
                new Stack { Resources = { {"Vetochka", 38}, {"Listik", 0}, {"Kamushek", 41}, {"Rosinka", 0} } },
                new Stack { Resources = { {"Vetochka", 24}, {"Listik", 35}, {"Kamushek", 23}, {"Rosinka", 0} } }
            };
            
            // colonies array
            List<Colony> colonies = new List<Colony>();
            List<Colony> coloniesTypes = new List<Colony>();

            // red colony
            coloniesTypes.Add(new Colony
            {
                Type = 0,
                Name = "красные",
                MaxWarriorsAmount = 5,
                MaxWorkersAmount = 15,
                Queen = new AntQueen {Name = "Шарлотта", Health = 24, Defence = 8, Damage = 21, NewQueenGrowth = new int[2] {3, 4}, NewQueenMax = new int[2] {2, 4} },
                Special = new NotAntSpecial {Name = "СПЕЦИАЛЬНЫЙ ленивый обычный агрессивный мстительный - Оса", Health = 27, Defence = 6, Damage = 13, CanAttack = true, AttackedTargetsAtOnce = 2, BitesWhileAttack = 2, KillsHisMurderer = true, CanTakeResources = false, Mods = new List<string>() { "не может брать ресурсы", "может быть атакован войнами", "убивает своего убийцу, даже если он неуязвим"} },
                WarriorTypes = new AntWarrior[3]
                {
                    new AntWarrior {Name = "ВОИН обычный", Health = 1, Defence = 0, Damage = 1, AttackedTargetsAtOnce = 1, BitesWhileAttack = 1, Mods = new List<string>() { "атакует своих вместо врагов" } },
                    new AntWarrior {Name = "ВОИН старший", Health = 2, Defence = 1, Damage = 2, AttackedTargetsAtOnce = 1, BitesWhileAttack = 1 },
                    new AntWarrior {Name = "ВОИН элитный аномальный", Health = 8, Defence = 4, Damage = 3, AttackedTargetsAtOnce = 2, BitesWhileAttack = 2, AttacksTeammates = true}
                },
                WorkerTypes = new AntWorker[3]
                {
                    new AntWorker {Name = "РАБОЧИЙ легендарный", Health = 10, Defence = 6, Damage = 0, Resources = new List<string>(){ "Vetochka",  "Listik" }, ResourcesAtOnce = 3, ResourceAndResource = true },
                    new AntWorker {Name = "РАБОЧИЙ обычный", Health = 1, Defence = 0, Damage = 0, Resources = new List<string>(){ "Kamushek" }, ResourcesAtOnce = 1, ResourceAndResource = false },
                    new AntWorker {Name = "РАБОЧИЙ продвинутый сильный", Health = 6, Defence = 2, Damage = 0, Resources = new List<string>(){ "Kamushek" }, ResourcesAtOnce = 2, ResourceAndResource = false }
                }
            });
            
            // green colony
            coloniesTypes.Add(new Colony
            {
                Type = 1,
                Name = "зелёные",
                MaxWarriorsAmount = 7,
                MaxWorkersAmount = 13,
                Queen = new AntQueen {Name = "Юлия", Health = 19, Defence = 8, Damage = 28, NewQueenGrowth = new int[2] {1, 3}, NewQueenMax = new int[2] {3, 3} },
                Special = new NotAntSpecial {Name = "СПЕЦИАЛЬНЫЙ трудолюбивый обычный мирный неповторимый - Оса", Health = 24, Defence = 6, CanTakeResources = true, CanAttack = false, Immortal = true, Resources = new List<string>(){ "Vetochka", "Listik", "Kamushek", "Rosinka" }, ResourcesAtOnce = 2, CanTakeInvisibleResources = true, Mods = new List<string>() { "может быть атакован войнами", "полностью неуязвим для всех атак (даже смертельных для неуязвимых)", "игнорирует все модификаторы врагов", "всегда находит нужный ресурс в куче, даже если его больше нет" } },
                WarriorTypes = new AntWarrior[3]
                {
                    new AntWarrior {Name = "ВОИН продвинутый", Health = 6, Defence = 2, Damage = 4, AttackedTargetsAtOnce = 2, BitesWhileAttack = 1 },
                    new AntWarrior {Name = "ВОИН элитный", Health = 8, Defence = 4, Damage = 3, AttackedTargetsAtOnce = 2, BitesWhileAttack = 2 },
                    new AntWarrior {Name = "ВОИН элитный злодей", Health = 8, Defence = 4, Damage = 3, AttackedTargetsAtOnce = 2, BitesWhileAttack = 2, InstantKill = true, Mods = new List<string>() { "всегда атакует последним и убивает с одного укуса любое насекомое даже неуязвимое" } }
                },
                WorkerTypes = new AntWorker[3]
                {
                    new AntWorker {Name = "РАБОЧИЙ элитный", Health = 8, Defence = 4, Damage = 0, Resources = new List<string>(){ "Kamushek", "Rosinka" }, ResourcesAtOnce = 3, ResourceAndResource = true },
                    new AntWorker {Name = "РАБОЧИЙ продвинутый", Health = 6, Defence = 2, Damage = 0, Resources = new List<string>(){ "Listik", "Rosinka" }, ResourcesAtOnce = 3, ResourceAndResource = false },
                    new AntWorker {Name = "РАБОЧИЙ продвинутый бригадир", Health = 6, Defence = 2, Damage = 0, Resources = new List<string>(){ "Listik", "Kamushek" }, ResourcesAtOnce = 3, ResourceAndResource = false, Mods = new List<string>() { "все рабочие могут брать +1 ресурс" } }
                }
            });
            for (var i = 0; i < coloniesTypes.Count; i++)
            {
                colonies.Add((Colony)coloniesTypes[i].Clone());
            }

            // units generating
            for (var i = 0; i < colonies.Count; i++)
            {
                colonies[i].AddUnits();
            }
            
            // making colonies - units dependencies, counting population
            for (var i = 0; i < colonies.Count; i++)
            {
                colonies[i].Queen.ItsColony = colonies[i];
                colonies[i].Special.ItsColony = colonies[i];
            }
            
            // sparrow - additional task
            Random sparrowRnd = new Random();
            var temporaryDay = sparrowRnd.Next(10);
            List<int> sparrowDays = new List<int>() { temporaryDay, temporaryDay + 1 };
            
            // choosing the way survival will be played
            bool survivalCycle = false;
            while (true)
            {
                int steps = 2;
                Console.WriteLine("Автоматическое (0) или пошаговое (1) выполнение? (при пошаговом выполнении можно запрашивать информацию о колониях и юнитах)");
                Int32.TryParse(Console.ReadLine(), out steps);
                if (steps == 0 || steps == 1)
                {
                    if (steps == 1) survivalCycle = true;
                    break;
                }
            }

            // survival
            for (var day = 0; day < 11; day++)
            {
                Console.WriteLine($"\nДень {day + 1} (до засухи {10 - day} дней)\n");
                while (survivalCycle)
                {
                    Console.WriteLine($"Продолжить выполнение (0) или посмотреть муравейники (номер колонии от 1 до {colonies.Count})?");
                    int typeFirst = -1;
                    Int32.TryParse(Console.ReadLine(), out typeFirst);
                    if (typeFirst != -1)
                    {
                        if (typeFirst == 0) break;
                        while (true)
                        {
                            Console.WriteLine("Общая информация (1), информация о конкретном юните (2), выход (0)");
                            int typeSecond = -1;
                            Int32.TryParse(Console.ReadLine(), out typeSecond);
                            if (typeSecond == 0 || typeSecond == 1 || typeSecond == 2)
                            {
                                if (typeSecond == 0) break;
                                if (typeSecond == 1) colonies[typeFirst - 1].LetUsIntroduceOurselves();
                                if (typeSecond == 2)
                                {
                                    while (true)
                                    {
                                        Console.WriteLine(
                                            $"Номер рабочего (от 1 до {colonies[typeFirst - 1].Workers.Count}), воина (от {colonies[typeFirst - 1].Workers.Count + 1} до {colonies[typeFirst - 1].Workers.Count + colonies[typeFirst - 1].Warriors.Count}), специального ({colonies[typeFirst - 1].Workers.Count + colonies[typeFirst - 1].Warriors.Count + 1}), выход(0)");
                                        int typeThird = -1;
                                        Int32.TryParse(Console.ReadLine(), out typeThird);
                                        if (typeThird >= 0 && typeThird <= colonies[typeFirst - 1].CountPopulation())
                                        {
                                            if (typeThird == 0) break;
                                            else if (typeThird <= colonies[typeFirst - 1].Workers.Count)
                                                colonies[typeFirst - 1].Workers[typeThird - 1].LetMeIntroduceMyself();
                                            else if (typeThird <= colonies[typeFirst - 1].Workers.Count +
                                                colonies[typeFirst - 1].Warriors.Count)
                                                colonies[typeFirst - 1]
                                                    .Warriors[typeThird - 1 - colonies[typeFirst - 1].Workers.Count]
                                                    .LetMeIntroduceMyself();
                                            else if (typeThird > colonies[typeFirst - 1].Workers.Count +
                                                colonies[typeFirst - 1].Warriors.Count)
                                                colonies[typeFirst - 1].Special.LetMeIntroduceMyself();
                                        }
                                    }
                                }
                            } 
                        }
                    }
                }
                // sparrow - additional task
                if (sparrowDays.Contains(day)) colonies[sparrowRnd.Next(colonies.Count)].Special = null;
                
                for (var cc = 0; cc < colonies.Count; cc++)
                {
                    var childrenAmount = 0;
                    foreach (var smallQueen in colonies[cc].Queen.SmallQueens) if (!smallQueen.Adult) childrenAmount++;
                    Console.WriteLine($"Колония '{colonies[cc].Name}':\n --- Королева '{colonies[cc].Queen.Name}', личинок: {childrenAmount} \n --- Ресурсы: веточка({colonies[cc].Resources["Vetochka"]}), листик({colonies[cc].Resources["Listik"]}), камушек({colonies[cc].Resources["Kamushek"]}), росинка({colonies[cc].Resources["Rosinka"]}) \n --- Популяция: {colonies[cc].CountPopulation()} (рабочие: {colonies[cc].Workers.Count}, воины: {colonies[cc].Warriors.Count}, специальные: {colonies[cc].CountPopulation() - colonies[cc].Warriors.Count - colonies[cc].Workers.Count})\n");
                    
                    Random rnd = new Random();
                    // small queens appear
                    for (var qc = 0; qc < colonies[cc].Queen.SmallQueens.Count; qc++)
                    {
                        if (!colonies[cc].Queen.SmallQueens[qc].Adult)
                        {
                            colonies[cc].Queen.SmallQueens[qc].Age++;
                            if (colonies[cc].Queen.SmallQueens[qc].Age == colonies[cc].Queen.NewQueenGrowth[rnd.Next(2)])
                            {
                                colonies[cc].Queen.SmallQueens[qc].Adult = true;
                                colonies.Add((Colony)coloniesTypes[colonies[cc].Queen.ItsColony.Type].Clone());
                                colonies[colonies.Count - 1].Name += " дочь №" + (colonies[cc].Queen.SmallQueens.IndexOf(colonies[cc].Queen.SmallQueens[qc]) + 1);
                                colonies[colonies.Count - 1].Queen = colonies[cc].Queen.SmallQueens[qc];
                                colonies[colonies.Count - 1].Queen.SmallQueens.Clear();
                                colonies[colonies.Count - 1].Warriors.Clear();
                                colonies[colonies.Count - 1].Workers.Clear();
                                colonies[colonies.Count - 1].AddUnits();
                                colonies[colonies.Count - 1].Resources = new Dictionary<string, int>() { {"Vetochka", 0}, {"Listik", 0}, {"Kamushek", 0}, {"Rosinka", 0} };
                            }
                        }
                    }
                    if (colonies[cc].Queen.CanMakeChildren)
                    {
                        if (rnd.Next(2) == 1 && colonies[cc].Queen.SmallQueens.Count < colonies[cc].Queen.NewQueenMax[1])
                        {
                            colonies[cc].Queen.SmallQueens.Add((AntQueen) colonies[cc].Queen.Clone());
                            colonies[cc].Queen.SmallQueens[colonies[cc].Queen.SmallQueens.Count - 1].SmallQueens = new List<AntQueen>();
                            colonies[cc].Queen.SmallQueens[colonies[cc].Queen.SmallQueens.Count - 1].Adult = false;
                            colonies[cc].Queen.SmallQueens[colonies[cc].Queen.SmallQueens.Count - 1].CanMakeChildren = false;
                            colonies[cc].Queen.SmallQueens[colonies[cc].Queen.SmallQueens.Count - 1].Age = 0;
                            colonies[cc].Queen.SmallQueens[colonies[cc].Queen.SmallQueens.Count - 1].Mother = colonies[cc].Queen;
                            colonies[cc].Queen.SmallQueens[colonies[cc].Queen.SmallQueens.Count - 1].Name += " (дочь)";
                        }
                    }
                    int stackNumber;
                    for (var uc = 0; uc < colonies[cc].Warriors.Count; uc++)
                    {
                        stackNumber = rnd.Next(stacks.Count);
                        stacks[stackNumber].WarriorsList.Add(colonies[cc].Warriors[uc]);
                        
                        // what colonies' units are on the stock right now
                        if (!stacks[stackNumber].ColoniesList.Contains(colonies[cc]))
                        {
                            stacks[stackNumber].ColoniesList.Add(colonies[cc]);
                        }
                    }
                    for (var uc = 0; uc < colonies[cc].Workers.Count; uc++)
                    {
                        stackNumber = rnd.Next(stacks.Count);
                        stacks[stackNumber].WorkersList.Add(colonies[cc].Workers[uc]);
                        
                        // what colonies' units are on the stock right now
                        if (!stacks[stackNumber].ColoniesList.Contains(colonies[cc]))
                        {
                            stacks[stackNumber].ColoniesList.Add(colonies[cc]);
                        }
                    }
                    stackNumber = rnd.Next(stacks.Count);
                    if (colonies[cc].Special != null)
                    {
                        stacks[stackNumber].SpecialsList.Add(colonies[cc].Special);
                    }

                    // what colonies' units are on the stock right now
                    if (!stacks[stackNumber].ColoniesList.Contains(colonies[cc]))
                    {
                        stacks[stackNumber].ColoniesList.Add(colonies[cc]);
                    }
                }
                for (var sc = 0; sc < stacks.Count; sc++)
                {
                    if (!stacks[sc].IsEmpty())
                    {
                        Console.WriteLine($"Куча {sc + 1}: веточка({stacks[sc].Resources["Vetochka"]}), листик({stacks[sc].Resources["Listik"]}), камушек({stacks[sc].Resources["Kamushek"]}), росинка({stacks[sc].Resources["Rosinka"]})");
                        if (stacks[sc].ColoniesList.Count > 1)
                        {
                            if (stacks[sc].WarriorsList.Count > 0 && stacks[sc].SpecialsList.Count == 0)
                                stacks[sc].BattleChecker();
                            else if (stacks[sc].SpecialsList.Count > 0 && stacks[sc].WarriorsList.Count == 0)
                                stacks[sc].BattleChecker();
                            else if (stacks[sc].WarriorsList.Count > 0 && stacks[sc].SpecialsList.Count > 0)
                                stacks[sc].BattleChecker();
                        }

                        for (var wc = 0; wc < stacks[sc].WorkersList.Count; wc++)
                        {
                            if (stacks[sc].WorkersList[wc].IsAlive) stacks[sc] = stacks[sc].WorkersList[wc].TakeResources(stacks[sc]);
                        }
                        for (var spc = 0; spc < stacks[sc].SpecialsList.Count; spc++)
                        {
                            if (stacks[sc].SpecialsList[spc].IsAlive && stacks[sc].SpecialsList[spc].CanTakeResources) stacks[sc] = stacks[sc].SpecialsList[spc].TakeResources(stacks[sc]);
                        }
                    }
                    else Console.WriteLine($"Куча {sc + 1}: истощена");
                    
                    // removing dead units from colonies
                    for (var cc1 = 0; cc1 < colonies.Count; cc1++)
                    {
                        for (var uc1 = 0; uc1 < stacks[sc].WarriorsList.Count; uc1++)
                        {
                            if (!stacks[sc].WarriorsList[uc1].IsAlive) colonies[cc1].Warriors.Remove(stacks[sc].WarriorsList[uc1]);
                        }
                        for (var uc1 = 0; uc1 < stacks[sc].WorkersList.Count; uc1++)
                        {
                            if (!stacks[sc].WorkersList[uc1].IsAlive) colonies[cc1].Workers.Remove(stacks[sc].WorkersList[uc1]);
                        }
                        for (var uc1 = 0; uc1 < stacks[sc].SpecialsList.Count; uc1++)
                        {
                            if (!stacks[sc].SpecialsList[uc1].IsAlive)
                            {
                                colonies[cc1].Special = null;
                                break;
                            }
                        }
                    }
                    stacks[sc].WarriorsList.Clear();
                    stacks[sc].WorkersList.Clear();
                    stacks[sc].SpecialsList.Clear();
                    stacks[sc].ColoniesList.Clear();
                }

                if (sparrowDays.Contains(day)) Console.WriteLine($"\nГлобальный эффект: Воробей нападает на случайную колонию перед походом и съедает особенное насекомое (в течение ещё {sparrowDays[1] - day} дней)\n");
            }

            Colony survivedColony = null;
            var maxResources = 0;
            for (var i = 0; i < colonies.Count; i++)
            {
                if (colonies[i].ResourcesCounter() > maxResources)
                {
                    maxResources = colonies[i].ResourcesCounter();
                    survivedColony = colonies[i];
                }
            }
            Console.WriteLine($"\nВыжившая колония: {survivedColony.Name} c {survivedColony.ResourcesCounter()} шт. ресурсов");
        }
    }
}