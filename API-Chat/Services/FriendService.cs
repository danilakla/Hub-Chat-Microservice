﻿using API_Chat.Data;
using API_Chat.DTO;
using API_Chat.Model;
using Microsoft.EntityFrameworkCore;

namespace API_Chat.Services
{
    public class FriendService : IFriendService
	{
		private readonly ApplicationContext applicationContext;
		private readonly IChatService chatService;

		public FriendService(ApplicationContext applicationContext, IChatService chatService)
		{
			this.applicationContext = applicationContext;
			this.chatService = chatService;
		}
		public async Task AddFriend(AcceptNoficationDTO acceptNoficationDTO)
		{
			try
			{
			var notificaiton = await applicationContext.Notifications
					.Include(e=>e.Contacts)
					.Where(e=>e.Id==acceptNoficationDTO.NotificationId)
					.FirstOrDefaultAsync();
			var initiator = await applicationContext.Contacts.Include(e=>e.Conversations)
					.Where(e => e.Email.Equals(notificaiton.FromWhom))
					.FirstOrDefaultAsync();

			var user = notificaiton.Contacts;
				string nameOfRoom = $"{user.Id}{initiator.Id}";
			var conversation = new Conversations
			{
				IsGroup = false,
				ConversationName = nameOfRoom,
				RoomName = ""};
			user.Conversations.Add(conversation);
			initiator.Conversations.Add(conversation);
				await chatService.CreateRoom(nameOfRoom);
				await applicationContext.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}

		}

		public async Task DeleteFriend(string roomName)
		{
			try
			{
				var conversation = await applicationContext.Conversations.Where(e=>e.ConversationName==roomName).FirstOrDefaultAsync();
				applicationContext.Conversations.Remove(conversation);
				await applicationContext.SaveChangesAsync();	
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<List<FriendDTO>> GetFriends(string email)
		{
			try
			{
				var user = await applicationContext.Contacts.Where(e => e.Email.Equals(email)).FirstOrDefaultAsync();

				var friends = await applicationContext.friendDTOs.FromSqlInterpolated<FriendDTO>($"select Name,LastName, Photo, ConversationName, Email ,Contacts.Id from ContactsConversations  JOIN Conversations ON ContactsConversations.ConversationsId=Conversations.Id JOIN Contacts ON ContactsConversations.ContactsId=Contacts.Id where ConversationName IN( Select ConversationName from Conversations as C Join ContactsConversations AS CC On CC.ConversationsId=C.Id where CC.ContactsId={user.Id} ) AND Contacts.Id  <> {user.Id} AND IsGroup=0").ToListAsync();

				return friends;
			}
			catch (Exception)
			{

				throw;
			}
			
		}
	}
}
