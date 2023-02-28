namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;

public class LiquidTemplateDataComparer : IEqualityComparer<ILiquidTemplateData>
{
    public bool Equals(ILiquidTemplateData? x, ILiquidTemplateData? y)
    {
        if (x == null && y == null)
            return true;
        else if (x == null && y != null)
            return false;
        else if (x != null && y == null)
            return false;

        return string.Equals(x.Key, y.Key, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode(ILiquidTemplateData obj)
    {
        if (obj == null)
            return 0;

        return obj.Key.ToLower().GetHashCode();
    }
}

public class LiquidTemplateDataContentComparer : IEqualityComparer<ILiquidTemplateData>
{
    public bool Equals(ILiquidTemplateData? x, ILiquidTemplateData? y)
    {
        if (x == null && y == null)
            return true;
        else if (x == null && y != null)
            return false;
        else if (x != null && y == null)
            return false;

        
        var test = string.Equals(x.Key, y.Key, StringComparison.InvariantCultureIgnoreCase) && string.Equals(x.Template, y.Template, StringComparison.InvariantCulture);



        return string.Equals(x.Key, y.Key, StringComparison.InvariantCultureIgnoreCase) && string.Equals(x.Template, y.Template, StringComparison.InvariantCulture);
    }

    public int GetHashCode(ILiquidTemplateData obj)
    {
        if (obj == null)
            return 0;

        if (obj?.Template == null)
            return obj.Key.ToLower().GetHashCode(); ;

        return obj.Key.ToLower().GetHashCode() + obj.Template.GetHashCode();
    }
}
