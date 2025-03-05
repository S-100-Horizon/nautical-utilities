namespace NauticalUtilitiesProAppModule.DynamicMenus
{
    internal class ButtonGeneral : ButtonQuery
    {
        protected override string Name => "General";

        //protected override string WhereClause => $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {700000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {700000} AND PLTS_COMP_SCALE < {3500000}))";
    }
}
