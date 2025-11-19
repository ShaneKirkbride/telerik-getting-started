namespace ConfigSetup.Application.OpenTap;

public interface IOpenTapSequenceParser
{
    OpenTapSequence Parse(string content);
}
