using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriviaSpark.Core.OpenTriviaDb
{
    /// <summary>
    /// Class to store the results of an HTTP GET call.
    /// </summary>
    public class HttpGetCallResults<T>
    {
        /// <summary>
        /// Default constructor to initialize the iteration and status path.
        /// </summary>
        public HttpGetCallResults()
        {
            Iteration = 0;
            RequestPath = string.Empty;
        }

        /// <summary>
        /// Constructor to initialize the iteration and status path from another instance of HttpGetCallResults.
        /// </summary>
        /// <param name="statusCall">An instance of HttpGetCallResults.</param>
        public HttpGetCallResults(HttpGetCallResults<T> statusCall)
        {
            Iteration = statusCall.Iteration;
            RequestPath = statusCall.RequestPath;
        }

        /// <summary>
        /// Constructor to initialize the iteration and status path from given values.
        /// </summary>
        /// <param name="it">Iteration number of the HTTP GET call.</param>
        /// <param name="path">Status path of the HTTP GET call.</param>
        public HttpGetCallResults(int it, string path)
        {
            Iteration = it;
            RequestPath = path;
        }

        /// <summary>
        /// Id for this record
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public int Id { get; set; }

        /// <summary>
        /// Property to store the completion date and time of the HTTP GET call.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:yyyy.MM.dd hh:mm:ss.ffff}")]
        public DateTime? CompletionDate { get; set; }

        /// <summary>
        /// Property to store the elapsed time in milliseconds of the HTTP GET call.
        /// </summary>
        public long ElapsedMilliseconds { get; set; }
        /// <summary>
        /// Error Message if something goes wrong, usually null
        /// </summary>
        public string? ErrorMessage { get; set; }
        /// <summary>
        /// Property to store the iteration number of the HTTP GET call.
        /// </summary>
        public int Iteration { get; set; }
        /// <summary>
        /// Number of retires to get a successful HTTP GET call.
        /// </summary>
        public int Retries { get; set; }
        /// <summary>
        /// Property to store the status path of the HTTP GET call.
        /// </summary>
        public string RequestPath { get; set; }

        /// <summary>
        /// Property to store the results of the HTTP GET call.
        /// </summary>
        [NotMapped]
        public T? ResponseResults { get; set; }
    }
}
