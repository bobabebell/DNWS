
namespace DNWS
{
    public interface IPlugin
    {
        void PreProcessing(HTTPRequest request);
        HTTPResponse PostProcessing(HTTPResponse response);
        HTTPResponse GetResponse(HTTPRequest request);
    }

}