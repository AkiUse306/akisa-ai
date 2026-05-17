#include <iostream>
#include <string>
#include <vector>
#include <sstream>

static std::vector<std::string> splitWords(const std::string &text)
{
    std::vector<std::string> tokens;
    std::istringstream stream(text);
    std::string word;
    while (stream >> word)
    {
        tokens.push_back(word);
    }
    return tokens;
}

static std::string summarizePrompt(const std::string &prompt)
{
    auto tokens = splitWords(prompt);
    if (tokens.size() < 8)
    {
        return "AKISA Core: I will help you build your AI platform with this prompt.";
    }

    return "AKISA Core: analyzing " + std::to_string(tokens.size()) + " tokens and generating a high-level reasoning plan.";
}

int main(int argc, char *argv[])
{
    std::cout << "AKISA-AI Core Engine initialized." << std::endl;
    std::cout << "Enter a prompt as arguments or run without arguments for default diagnostics." << std::endl;

    if (argc <= 1)
    {
        std::cout << "Usage: akisa_core <prompt>" << std::endl;
        return 0;
    }

    std::string prompt;
    for (int i = 1; i < argc; ++i)
    {
        prompt += argv[i];
        if (i + 1 < argc)
        {
            prompt += " ";
        }
    }

    std::cout << summarizePrompt(prompt) << std::endl;
    std::cout << "Key capabilities: reasoning, memory-aware planning, multi-agent orchestration." << std::endl;
    return 0;
}
