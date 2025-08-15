using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.UnitTest.Builders;
using Xunit;

namespace PlantBasedPizza.UnitTest
{
    public class KitchenTests
    {
        internal const string OrderIdentifier = "ORDER123";
        
        [Fact]
        public void CanCreateNewKitchenRequest_ShouldCreate()
        {
            var request = new KitchenRequestBuilder().AddRecipe("Pizza").Build();

            request.Recipes.Count.Should().Be(1);
            request.OrderIdentifier.Should().Be(OrderIdentifier);
            request.OrderState.Should().Be(OrderState.NEW);
            request.BakeCompleteOn.Should().BeNull();
            request.OrderReceivedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            request.PrepCompleteOn.Should().BeNull();
            request.QualityCheckCompleteOn.Should().BeNull();
            request.KitchenRequestId.Should().NotBeNull();
        }
        
        [Fact]
        public void CanCreateWithNullOrderIdentifier_ShouldError()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new KitchenRequest(null, new List<RecipeAdapter>(1)
                {
                    new RecipeAdapter("Pizza")
                });
            });
        }

        [Fact]
        public void CanCreateAndMarkPreparing_ShouldSetPrepCompleted()
        {
            var request = new KitchenRequestBuilder().AddRecipe("Pizza").Build();
            
            request.StartPreparing();
            request.CompletePreparing();

            request.PrepCompleteOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void CanCreateAndMarkBaked_ShouldSetBakeComplete()
        {
            var request = new KitchenRequestBuilder().AddRecipe("Pizza").Build();
            
            request.StartPreparing();
            request.CompletePreparing();
            request.CompleteBaking();

            request.BakeCompleteOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void CanCreateAndMarkQualityChecked_ShouldSetQualityCheckedOn()
        {
            var request = new KitchenRequestBuilder().AddRecipe("Pizza").Build();
            
            request.StartPreparing();
            request.CompletePreparing();
            request.CompleteBaking();
            request.CompleteQualityCheck();

            request.QualityCheckCompleteOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}