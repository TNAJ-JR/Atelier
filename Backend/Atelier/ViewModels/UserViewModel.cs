namespace Atelier.ViewModels;

public record UserViewModel(string Id, string Name, string Email, string Nom, string Prenom, IList<string> Roles);
