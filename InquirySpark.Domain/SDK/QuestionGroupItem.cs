namespace InquirySpark.Domain.SDK;

public class QuestionGroupItem
{
    public int QuestionGroupID { get; set; }
    public int SurveyID { get; set; }
    public string QuestionGroupNM { get; set; }
    public string QuestionGroupShortNM { get; set; }
    public string QuestionGroupDS { get; set; }
    public int QuestionGroupOrder { get; set; }
    public decimal QuestionGroupWeight { get; set; }
    public string QuestionGroupHeader { get; set; }
    public string QuestionGroupFooter { get; set; }
    public decimal? DependentMaxScore { get; set; }
    public decimal? DependentMinScore { get; set; }
    public int? DependentQuestionGroupID { get; set; }
    public List<QuestionGroupMemberItem> QuestionMembership { get; set; } = [];
    public int ModifiedID { get; set; }
    public bool MarkedForDeletion { get; set; } = false;
}