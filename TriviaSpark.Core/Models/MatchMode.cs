namespace TriviaSpark.Core.Models;

public enum MatchMode
{
    Unknown = 0,
    OneAndDone, // User gets one chance to answer each question
    Sequential, // Questions are presented in the order they appear in the list
    Random, // Questions are presented in random order
    Adaptive // Difficulty of questions changes based on user performance
}

