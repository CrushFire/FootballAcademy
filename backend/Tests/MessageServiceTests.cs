using Application.Services;
using Core.Entities;
using Core.Enums;
using Core.Models.BroadcastModel;
using Core.Models.MessageModel;
using Xunit;

namespace Tests
{
    public class MessageServiceTests
    {
        private MessageService CreateService(string db)
        {
            var context = TestHelper.CreateContext(db);
            return new MessageService(context, TestHelper.CreateMapper());
        }

        // --- базовые ---

        [Fact]
        public async Task SendMessage_ПолучательНеНайден_404()
        {
            var service = CreateService(nameof(SendMessage_ПолучательНеНайден_404));
            var result = await service.SendAsync(1, SenderRole.Trainer, new MessageSendRequest { ReceiverId = 999, Text = "Привет" });
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task SendMessage_Успешно()
        {
            var context = TestHelper.CreateContext(nameof(SendMessage_Успешно));
            context.Users.Add(new User { Id = 1, Login = "sender", Email = "s@s.com", Role = "trainer", Password = "hash" });
            context.Users.Add(new User { Id = 2, Login = "receiver", Email = "r@r.com", Role = "admin", Password = "hash" });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.SendAsync(1, SenderRole.Trainer, new MessageSendRequest { ReceiverId = 2, Text = "Привет" });

            Assert.True(result.IsSuccess);
            Assert.Equal("Привет", result.Data!.Text);
        }

        [Fact]
        public async Task MarkRead_НеПолучатель_403()
        {
            var context = TestHelper.CreateContext(nameof(MarkRead_НеПолучатель_403));
            context.Messages.Add(new Message { Id = 1, SenderId = 1, ReceiverId = 2, Text = "X", SenderRole = SenderRole.Trainer });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.MarkReadAsync(1, 99);

            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task MarkRead_Успешно()
        {
            var context = TestHelper.CreateContext(nameof(MarkRead_Успешно));
            context.Messages.Add(new Message { Id = 1, SenderId = 1, ReceiverId = 2, Text = "X", SenderRole = SenderRole.Trainer });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.MarkReadAsync(1, 2);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task SendBroadcast_Дубль_409()
        {
            var context = TestHelper.CreateContext(nameof(SendBroadcast_Дубль_409));
            context.Broadcasts.Add(new Broadcast
            {
                Id = 1, Title = "Отмена", Text = "Текст",
                CreatedById = 1, CreatedByRole = SenderRole.Trainer,
                TargetType = BroadcastTargetType.All, CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.SendBroadcastAsync(1, SenderRole.Trainer, new BroadcastCreateRequest
            {
                Title = "Отмена", Text = "Текст", TargetType = BroadcastTargetType.All
            });

            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task GetBroadcastDetails_НесуществующийId_404()
        {
            var service = CreateService(nameof(GetBroadcastDetails_НесуществующийId_404));
            var result = await service.GetBroadcastDetailsAsync(999, null);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetDialog_ВозвращаетТолькоСообщенияМеждуДвумя()
        {
            var context = TestHelper.CreateContext(nameof(GetDialog_ВозвращаетТолькоСообщенияМеждуДвумя));
            context.Messages.Add(new Message { Id = 1, SenderId = 1, ReceiverId = 2, Text = "A", SenderRole = SenderRole.Trainer });
            context.Messages.Add(new Message { Id = 2, SenderId = 2, ReceiverId = 1, Text = "B", SenderRole = SenderRole.Admin });
            context.Messages.Add(new Message { Id = 3, SenderId = 3, ReceiverId = 1, Text = "C", SenderRole = SenderRole.Medical });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.GetDialogAsync(1, 2, null);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data!.Messages.Count);
        }

        // --- нестандартные данные ---

        [Fact]
        public async Task SendMessage_ОченьДлинныйТекст_Success()
        {
            var context = TestHelper.CreateContext(nameof(SendMessage_ОченьДлинныйТекст_Success));
            context.Users.Add(new User { Id = 1, Login = "s", Email = "s@s.com", Role = "trainer", Password = "h" });
            context.Users.Add(new User { Id = 2, Login = "r", Email = "r@r.com", Role = "admin", Password = "h" });
            await context.SaveChangesAsync();

            var longText = new string('А', 5000);
            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.SendAsync(1, SenderRole.Trainer, new MessageSendRequest
            {
                ReceiverId = 2,
                Text = longText
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(longText, result.Data!.Text);
        }

        [Fact]
        public async Task SendMessage_СпецСимволыВТексте_Success()
        {
            var context = TestHelper.CreateContext(nameof(SendMessage_СпецСимволыВТексте_Success));
            context.Users.Add(new User { Id = 1, Login = "s", Email = "s@s.com", Role = "trainer", Password = "h" });
            context.Users.Add(new User { Id = 2, Login = "r", Email = "r@r.com", Role = "admin", Password = "h" });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.SendAsync(1, SenderRole.Trainer, new MessageSendRequest
            {
                ReceiverId = 2,
                Text = "Привет! <script>alert('xss')</script> & \"кавычки\" 🎉"
            });

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task SendMessage_МедицинскаяРоль_Success()
        {
            var context = TestHelper.CreateContext(nameof(SendMessage_МедицинскаяРоль_Success));
            context.Users.Add(new User { Id = 1, Login = "doc", Email = "doc@s.com", Role = "medical", Password = "h" });
            context.Users.Add(new User { Id = 2, Login = "player", Email = "p@r.com", Role = "admin", Password = "h" });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.SendAsync(1, SenderRole.Medical, new MessageSendRequest
            {
                ReceiverId = 2,
                Text = "Медицинское заключение"
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(SenderRole.Medical, result.Data!.SenderRole);
        }

        [Fact]
        public async Task GetDialog_ПустойДиалог_ВозвращаетПустойСписок()
        {
            var service = CreateService(nameof(GetDialog_ПустойДиалог_ВозвращаетПустойСписок));
            var result = await service.GetDialogAsync(1, 2, null);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!.Messages);
        }

        [Fact]
        public async Task GetDialog_МногоСообщений_ВозвращаетВсе()
        {
            var context = TestHelper.CreateContext(nameof(GetDialog_МногоСообщений_ВозвращаетВсе));
            for (int i = 1; i <= 50; i++)
                context.Messages.Add(new Message { Id = i, SenderId = 1, ReceiverId = 2, Text = $"Сообщение {i}", SenderRole = SenderRole.Trainer });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.GetDialogAsync(1, 2, null);

            Assert.True(result.IsSuccess);
            Assert.Equal(50, result.Data!.Messages.Count);
        }

        [Fact]
        public async Task MarkRead_УжеПрочитанное_Success()
        {
            var context = TestHelper.CreateContext(nameof(MarkRead_УжеПрочитанное_Success));
            context.Messages.Add(new Message { Id = 1, SenderId = 1, ReceiverId = 2, Text = "X", SenderRole = SenderRole.Trainer, IsRead = true });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.MarkReadAsync(1, 2);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task MarkRead_НесуществующееСообщение_404()
        {
            var service = CreateService(nameof(MarkRead_НесуществующееСообщение_404));
            var result = await service.MarkReadAsync(999, 1);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task SendBroadcast_РазныеАвторы_НеДубль_Success()
        {
            var context = TestHelper.CreateContext(nameof(SendBroadcast_РазныеАвторы_НеДубль_Success));
            // тот же Title, но другой автор — не дубль
            context.Broadcasts.Add(new Broadcast
            {
                Id = 1, Title = "Отмена", Text = "Текст",
                CreatedById = 1, CreatedByRole = SenderRole.Trainer,
                TargetType = BroadcastTargetType.All, CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.SendBroadcastAsync(2, SenderRole.Trainer, new BroadcastCreateRequest
            {
                Title = "Отмена", Text = "Текст", TargetType = BroadcastTargetType.All
            });

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetBroadcasts_ПустаяБД_ВозвращаетПустойСписок()
        {
            var service = CreateService(nameof(GetBroadcasts_ПустаяБД_ВозвращаетПустойСписок));
            var result = await service.GetBroadcastsAsync(null);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetBroadcastDetails_БезПолучателей_ПустойСписок()
        {
            var context = TestHelper.CreateContext(nameof(GetBroadcastDetails_БезПолучателей_ПустойСписок));
            context.Broadcasts.Add(new Broadcast
            {
                Id = 1, Title = "Тест", Text = "Текст",
                CreatedById = 1, CreatedByRole = SenderRole.Trainer,
                TargetType = BroadcastTargetType.All
            });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.GetBroadcastDetailsAsync(1, null);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!.Recipients);
        }
    }
}
