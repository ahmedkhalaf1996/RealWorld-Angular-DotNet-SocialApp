using backend.Models;

namespace backend.Services
{
    public interface IPostService {
        Task CreateOnePostAsync(Post post);
        Task<Post?> UpdatePost(string id, Post newPost);
        Task<PostResponse?> GetPostByID(string id);
        Task<User?> GetUsByid(string id);
        Task DeletePostAsync(string id);
        Task<(List<Post>, List<User>)> Search(string searchQuery);

        Task<Object> Query(List<string> ides, int? queryPage);

        Task<Comment> CreateComment(string postId, string userId, string value);

        Task<bool> DeleteComment(string commentId, string userId, string postCreatorId);

        Task<PostResponse?> GetPostWithComments(string postId);
    }
}