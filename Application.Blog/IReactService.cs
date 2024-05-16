using Domain.Blog.dto;
using Domain.Blog.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Blog
{
    // Interface for managing reactions on blogs.
    public interface IReactService
    {
        // Creates a new reaction.
        Task<string> createReact(ReactDto reactDto);

        // Updates an existing reaction.
        Task<bool> updateReact(ReactDto react, Guid react_id);

        // Retrieves all reactions.
        Task<List<React>> getAllReacts();

        // Retrieves a reaction by its ID.
        Task<React> getReactById(ReactDto reactDto, Guid react_id);

        // Deletes a reaction.
        Task<bool> deleteReact(ReactDto reactDto, Guid react_id);

        // Retrieves the total number of reactions.
        Task<object> getNoOfReact();

        // Retrieves the number of reactions within a specified date range.
        Task<object> getNoOfReactByDate(string from, string to);
    }
}
