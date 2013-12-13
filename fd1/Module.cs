using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApplicationFramework.Contracts;
using ApplicationFramework.Services;
using nVentive.Umbrella.Concurrency;
using nVentive.Umbrella;
using nVentive.Umbrella.Patterns.IoC;
using YouSendIt.Business.Contracts;
using YouSendIt.Business.Services;
using YouSendIt.Client.Contracts;
using nVentive.Umbrella.Services.Contract.BackgroundTransfer;
using nVentive.Umbrella.Services.Contract.Settings;
using nVentive.Umbrella.Patterns.Serialization;
#if WINRT
using nVentive.Umbrella.WinRT.Utilities.Services.Notifications;
using nVentive.Umbrella.WinRT.Utilities.Status;
using nVentive.Umbrella.Utilities.Tasks.Browser;

#endif

namespace YouSendIt.Business
{
	public static class Module
	{
		public static void Initialize(Container container)
		{
#if false
			InitializeFast(container);
#else
			// Register the generated resolver, from the content of the InitializeFast method
			container.AddResolver(new FastResolver());
#endif
			// Do not register services this section, unless the code included in
			// InitializeFast() does not compile due to invalid code generation.
			container.Register<IChangeNotificationService<long>>("FolderChangedService", c => new ChangeNotificationService<long>());
			container.Register<IChangeNotificationService<long>>("ParticipantsChangedService", c => new ChangeNotificationService<long>());
			container.Register<IChangeNotificationService<string>>("TrackingFileChangedService", c => new ChangeNotificationService<string>());

			// REFACTOR : this suppose that must services are already referenced. This design should be improved
			var uploadService = new UploadBusinessService(container.Resolve<IUploadClientService>(),
														  container.Resolve<IBackgroundTransferService>(),
														  container.ResolveNamed<IChangeNotificationService<long>>("FolderChangedService"));
			container.Register<IUploadBusinessService>(uploadService);

#if NETFX_CORE
			container.Register<IAccountsBusinessServiceExtended>(c => new AccountsBusinessServiceExtended(c.Resolve<IAccountsBusinessService>(), c.Resolve<ILoginBusinessService>()));
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Service registration cannot be refactored in InitializeFast")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Service registration cannot be refactored in InitializeFast")]
		private static void InitializeFast(Container container)
		{
			// Register business services here
			container.Register<IShareBusinessService>(c =>
				new ShareBusinessService(
					c.Resolve<IShareClientService>(),
					c.Resolve<IPoliciesBusinessService>(),
					c.ResolveNamed<IChangeNotificationService<long>>("ParticipantsChangedService"),
					c.ResolveNamed<IChangeNotificationService<long>>("FolderChangedService")));
			container.Register<ISignatureBusinessService>(c => 
				new SignatureBusinessService(
					c.Resolve<ISignatureClientService>(),
					c.Resolve<IUploadClientService>(),
					c.Resolve<IApplicationSettingsService>()));
			container.Register<IFoldersBusinessService>(c => 
				new FoldersBusinessService(
					c.Resolve<IFoldersClientService>(), 
					c.ResolveNamed<IChangeNotificationService<long>>("FolderChangedService")));
			container.Register<IFilesBusinessService>(c => 
				new FilesBusinessService(
					c.Resolve<IFilesClientService>(),
					c.Resolve<IPoliciesBusinessService>(),
					c.ResolveNamed<IChangeNotificationService<long>>("FolderChangedService")));
			container.Register<ILoginBusinessService>(c => 
				new LoginBusinessService(
					c.Resolve<ILoginClientService>(), 
					c.Resolve<IIdentificationClientService>(),
					c.Resolve<IPoliciesBusinessService>()));
			container.Register<IAccountsBusinessService>(c => new AccountsBusinessService(c.Resolve<IAccountsClientService>()));
			container.Register<IContactPickerBusinessService>(c => new ContactPickerBusinessService(c.Resolve<IEmailChooserService>()));
			container.Register<IPoliciesBusinessService>(c => new PoliciesBusinessService(c.Resolve<IPoliciesClientService>()));
			container.Register<IApplicationSettingsBusinessService>(c => 
				new ApplicationSettingsBusinessService(
					c.Resolve<IApplicationSettingsService>(),
					c.Resolve<IAccountsClientService>()));
			container.Register<ITrackingBusinessService>(c => new TrackingBusinessService(c.Resolve<ITrackingClientService>()));
			container.Register<IPreviewService>(c =>
				new PreviewService(
					c.Resolve<IFilesBusinessService>(),
					c.Resolve<ISignatureBusinessService>(),
					c.Resolve<ISerializer>()));
			container.Register<IOemAuthenticationBusinessService>(c =>
				new OemAuthenticationBusinessService(
					c.Resolve<IOemAuthenticationClientService>(),
					c.Resolve<IIdentificationClientService>(),
					c.Resolve<IAccountsBusinessService>()
					)
				);

#if WINDOWS_PHONE
			container.Register<IRecentlyViewedBusinessService>(c => 
				new RecentlyViewedBusinessService(
					c.Resolve<IApplicationSettingsService>()));
#endif

#if WINRT
			container.Register<IBrowser>(c => new Browser(c.Resolve<ISchedulers>().Dispatcher));

			container.Register<ISrtSendBusinessService>(c =>
							new SrtSendBusinessService(
								c.Resolve<ISrtSendClientService>()));

			container.Register<IBackgroundTransferBusinessService>(c =>
				new BackgroundTransferBusinessService(
					c.Resolve<IBackgroundTransferService>(),
					c.Resolve<IUploadClientService>(), 
					c.Resolve<IToastService>(), 
					c.Resolve<IStatusAggregator>(),
					Schedulers.Default,
					c.ResolveNamed<IChangeNotificationService<long>>("FolderChangedService"),
					c.Resolve<ISrtSendBusinessService>()));


			container.Register<IDecorateFullViewService>(c => new DecorateFullViewService());
#endif
		}
	}
}
