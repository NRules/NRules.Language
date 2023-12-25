namespace GettingStarted.Domain;

public class Order
{
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
    public double PercentDiscount { get; set; }
    public bool IsDiscounted => PercentDiscount > 0;
    public double Price => UnitPrice*Quantity*(1.0 - PercentDiscount/100.0);
}