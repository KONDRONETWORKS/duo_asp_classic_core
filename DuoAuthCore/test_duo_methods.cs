// Ce fichier ne doit pas contenir de point d'entrée (Main) pour éviter l'erreur CS0017.
// Veuillez déplacer ce code dans un autre fichier ou supprimer la méthode Main ici.
// Si vous souhaitez conserver ce code pour l'exploration, vous pouvez le transformer en une méthode statique utilitaire.

using System;
using System.Reflection;
using DuoUniversal;

public static class DuoUniversalExplorer
{
    public static void ExplorerClient()
    {
        Console.WriteLine("=== EXPLORATION DES MÉTHODES DUOUNIVERSAL ===");
        
        // Obtenir le type Client
        Type clientType = typeof(Client);
        Console.WriteLine($"Type Client: {clientType.FullName}");
        
        // Lister toutes les méthodes publiques
        MethodInfo[] methods = clientType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        Console.WriteLine("\nMéthodes publiques disponibles:");
        foreach (var method in methods)
        {
            Console.WriteLine($"- {method.Name}");
        }
        
        // Lister toutes les propriétés publiques
        PropertyInfo[] properties = clientType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Console.WriteLine("\nPropriétés publiques disponibles:");
        foreach (var property in properties)
        {
            Console.WriteLine($"- {property.Name}: {property.PropertyType.Name}");
        }
        
        Console.WriteLine("\n=== FIN EXPLORATION ===");
    }
}