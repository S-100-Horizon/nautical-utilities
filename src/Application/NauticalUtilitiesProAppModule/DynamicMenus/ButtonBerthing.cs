namespace NauticalUtilitiesProAppModule.DynamicMenus
{
    internal class ButtonBerthing : ButtonQuery
    {
        protected override string Name => "Berthing";

        //protected override string WhereClause => $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {12000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE < {12000}))";
    }
}
