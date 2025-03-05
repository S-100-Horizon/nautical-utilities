namespace NauticalUtilitiesProAppModule.DynamicMenus
{
    internal class ButtonCoastal : ButtonQuery
    {
        protected override string Name => "Coastal";

        //protected override string WhereClause => $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {180000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {180000} AND PLTS_COMP_SCALE < {700000}))";
    }
}
