namespace NauticalUtilitiesProAppModule.DynamicMenus
{
    internal class ButtonApproach : ButtonQuery
    {
        protected override string Name => "Approach";

        //protected override string WhereClause => $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {45000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {45000} AND PLTS_COMP_SCALE < {180000}))";
    }
}
