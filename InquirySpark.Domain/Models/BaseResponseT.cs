namespace InquirySpark.Domain.Models
{

    /// <summary>
    /// Class BaseResponse.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseResponse<T>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResponse{T}"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        protected BaseResponse()
        {
            IsSuccessful = false;
            Data = default;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResponse{T}"/> class.
        /// </summary>
        /// <param name="data">The resource.</param>
        public BaseResponse(T? data)
        {
            IsSuccessful = true;
            Data = data;
            return;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResponse{T}"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        protected BaseResponse(string message)
        {
            IsSuccessful = false;
            Errors.Add(message);
            Data = default;
        }

        public BaseResponse(string[] errors)
        {
            IsSuccessful = false;
            Errors.AddRange(errors);
            Data = default;
        }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        /// <value>The resource.</value>
        public T? Data { get; private set; }

        public string Error
        {
            get
            {
                return string.Join(Environment.NewLine, Errors);
            }
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public List<string> Errors { get; set; } = [];

        /// <summary>
        /// Gets a value indicating whether this <see cref="BaseResponse{T}"/> is success.
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool IsSuccessful { get; private set; }



    }
}