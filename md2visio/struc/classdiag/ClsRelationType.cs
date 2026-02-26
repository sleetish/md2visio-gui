namespace md2visio.struc.classdiag
{
    internal enum ClsRelationType
    {
        Inheritance,    // <|--  Solid triangle arrow + solid line
        Composition,    // *--   Solid diamond + solid line
        Aggregation,    // o--   Hollow diamond + solid line
        Association,    // -->   Normal arrow + solid line
        Dependency,     // ..>   Normal arrow + dashed line
        Realization,    // ..|>  Hollow triangle arrow + dashed line
        Link,           // --    No arrow solid line
        DashedLink      // ..    No arrow dashed line
    }

    internal enum ClsVisibility
    {
        Public,         // +
        Private,        // -
        Protected,      // #
        Internal        // ~
    }
}
