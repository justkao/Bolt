

using Bolt;
using Bolt.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestService.Core;
using TestService.Core.Parameters;

namespace TestService.Core
{
    public partial class PersonRepositoryExecutor : Bolt.Server.Executor, IExecutor
    {
        public override void Init()
        {
            AddAction(TestService.Core.PersonRepositoryDescriptor.UpdatePerson, PersonRepository_UpdatePerson);
            AddAction(TestService.Core.PersonRepositoryDescriptor.DoNothingAsAsync, PersonRepository_DoNothingAsAsync);
            AddAction(TestService.Core.PersonRepositoryDescriptor.DoNothing, PersonRepository_DoNothing);
            AddAction(TestService.Core.PersonRepositoryDescriptor.DoNothingWithComplexParameterAsAsync, PersonRepository_DoNothingWithComplexParameterAsAsync);
            AddAction(TestService.Core.PersonRepositoryDescriptor.DoNothingWithComplexParameter, PersonRepository_DoNothingWithComplexParameter);
            AddAction(TestService.Core.PersonRepositoryDescriptor.GetSimpleType, PersonRepository_GetSimpleType);
            AddAction(TestService.Core.PersonRepositoryDescriptor.GetSimpleTypeAsAsync, PersonRepository_GetSimpleTypeAsAsync);
            AddAction(TestService.Core.PersonRepositoryDescriptor.GetSinglePerson, PersonRepository_GetSinglePerson);
            AddAction(TestService.Core.PersonRepositoryDescriptor.GetSinglePersonAsAsync, PersonRepository_GetSinglePersonAsAsync);
            AddAction(TestService.Core.PersonRepositoryDescriptor.GetManyPersons, PersonRepository_GetManyPersons);
            AddAction(TestService.Core.PersonRepositoryDescriptor.GetManyPersonsAsAsync, PersonRepository_GetManyPersonsAsAsync);

            base.Init();
        }

        private async Task PersonRepository_UpdatePerson(ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<UpdatePersonParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            var result = instance.UpdatePerson(parameters.Person);
            await ResponseHandler.Handle(context, result);
        }

        private async Task PersonRepository_DoNothingAsAsync(ServerExecutionContext context)
        {
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            await instance.DoNothingAsAsync();
            await ResponseHandler.Handle(context);
        }

        private async Task PersonRepository_DoNothing(ServerExecutionContext context)
        {
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            instance.DoNothing();
            await ResponseHandler.Handle(context);
        }

        private async Task PersonRepository_DoNothingWithComplexParameterAsAsync(ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<DoNothingWithComplexParameterAsAsyncParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            await instance.DoNothingWithComplexParameterAsAsync(parameters.Person);
            await ResponseHandler.Handle(context);
        }

        private async Task PersonRepository_DoNothingWithComplexParameter(ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<DoNothingWithComplexParameterParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            instance.DoNothingWithComplexParameter(parameters.Person);
            await ResponseHandler.Handle(context);
        }

        private async Task PersonRepository_GetSimpleType(ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<GetSimpleTypeParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            var result = instance.GetSimpleType(parameters.Arg);
            await ResponseHandler.Handle(context, result);
        }

        private async Task PersonRepository_GetSimpleTypeAsAsync(ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<GetSimpleTypeAsAsyncParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            await instance.GetSimpleTypeAsAsync(parameters.Arg);
            await ResponseHandler.Handle(context);
        }

        private async Task PersonRepository_GetSinglePerson(ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<GetSinglePersonParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            var result = instance.GetSinglePerson(parameters.Person);
            await ResponseHandler.Handle(context, result);
        }

        private async Task PersonRepository_GetSinglePersonAsAsync(ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<GetSinglePersonAsAsyncParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            var result = await instance.GetSinglePersonAsAsync(parameters.Person);
            await ResponseHandler.Handle(context, result);
        }

        private async Task PersonRepository_GetManyPersons(ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<GetManyPersonsParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            var result = instance.GetManyPersons(parameters.Person);
            await ResponseHandler.Handle(context, result);
        }

        private async Task PersonRepository_GetManyPersonsAsAsync(ServerExecutionContext context)
        {
            var parameters = await DataHandler.ReadParametersAsync<GetManyPersonsAsAsyncParameters>(context);
            var instance = await InstanceProvider.GetInstanceAsync<IPersonRepository>(context);
            var result = await instance.GetManyPersonsAsAsync(parameters.Person);
            await ResponseHandler.Handle(context, result);
        }
    }
}

