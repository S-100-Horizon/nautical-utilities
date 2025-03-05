namespace NauticalUtilitiesProAppModule.DynamicMenus
{
    internal class ButtonHarbour : ButtonQuery
    {
        protected override string Name => "Harbour";

        //protected override string WhereClause => $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {12000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {12000} AND PLTS_COMP_SCALE < {45000}))";
    }
}
