using CotelNet.Domain.Common;

namespace CotelNet.Domain.Estafetas;

public sealed class Estafeta : Entity
{
    private Estafeta() { }

    public Estafeta(string codigo, string nombre)
    {
        Codigo = Require(codigo, nameof(codigo), 20);
        Nombre = Require(nombre, nameof(nombre), 150);
        Activa = true;
    }

    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public bool Activa { get; private set; }

    public void Update(string codigo, string nombre, bool activa)
    {
        Codigo = Require(codigo, nameof(codigo), 20);
        Nombre = Require(nombre, nameof(nombre), 150);
        Activa = activa;
        MarkUpdated();
    }

    private static string Require(string value, string name, int maxLength)
    {
        value = value?.Trim() ?? string.Empty;
        if (value.Length is 0 || value.Length > maxLength)
            throw new ArgumentException($"{name} debe contener entre 1 y {maxLength} caracteres.", name);
        return value;
    }
}
