namespace Pat.Mapping
{
    public static class MappingExtensions
    {
        private static readonly Mapper Mapper = new Mapper();

        public static TDest MapTo<TDest>(this object source) where TDest : class, new()
        {
            var dest = new TDest();
            Mapper.Map(source, dest);
            return dest;
        }

        public static void MapTo<TSource, TDest>(this TSource source, TDest dest)
        {
            Mapper.Map(source, dest);
        }
    }
}