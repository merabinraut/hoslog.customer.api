using hoslog.customer.shared;

namespace hoslog.customer.common.Utilities;

public class CommonApiResponseBuilder
{
    private readonly CommonApiResponse _response = new();

    public static CommonApiResponseBuilder Create()
    {
        return new CommonApiResponseBuilder();
    }

    public CommonApiResponseBuilder WithCode(string code)
    {
        _response.code = code;
        return this;
    }

    public CommonApiResponseBuilder WithMessage(string message)
    {
        _response.message = message;
        return this;
    }

    public CommonApiResponseBuilder WithData(object? data)
    {
        _response.data = data;
        return this;
    }

    public CommonApiResponseBuilder WithErrors(List<Errors>? errors)
    {
        _response.errors = errors;
        return this;
    }

    public CommonApiResponseBuilder AddError(string errorCode, string errorMessage)
    {
        _response.errors ??= new List<Errors>();
        _response.errors.Add(new Errors
        {
            errorCode = errorCode,
            errorMessage = errorMessage
        });
        return this;
    }

    public CommonApiResponse Build()
    {
        return _response;
    }
}