using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Utils.Cache {
    public static class ReportCache
    {
        public static void Set(string key, ReportDocumentation value)
        {
            CacheHandler<string, ReportDocumentation>.Set(key, value);
        }
        public static ReportDocumentation? Get(string key){
            return CacheHandler<string, ReportDocumentation>.Get(key);
        }
        public static bool Remove(string key){
            return CacheHandler<string, ReportDocumentation>.Remove(key);
        }
        public static void Clear(){
            CacheHandler<string, ReportDocumentation>.Clear();
        }
    }
}