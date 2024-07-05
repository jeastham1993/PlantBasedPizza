import com.amazonaws.services.lambda.runtime.events.SQSEvent;
import com.recipe.core.IRecipeRepository;
import com.recipe.core.Recipe;
import com.recipe.functions.FunctionConfiguration;
import com.recipe.functions.services.EventHandlerService;
import com.recipe.functions.services.IEventHandlerService;
import jakarta.annotation.Resource;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.*;

import java.util.ArrayList;
import java.util.Optional;

import static org.assertj.core.api.Assertions.assertThat;

public class FunctionTest {
    private String data = "{\"version\":\"0\",\"id\":\"440f76ac-7d40-6e9f-288f-f28de28c6c12\",\"detail-type\":\"test\",\"source\":\"test\",\"account\":\"730335273443\",\"time\":\"2024-07-05T15:06:22Z\",\"region\":\"eu-west-1\",\"resources\":[],\"detail\":{\"specversion\":\"1.0\",\"type\":\"com.example.someevent\",\"source\":\"/mycontext\",\"id\":\"A234-1234-1234\",\"time\":\"2018-04-05T17:31:00Z\",\"comexampleextension1\":\"value\",\"comexampleothervalue\":5,\"datacontenttype\":\"application/json\",\"ddtraceid\":\"123456\",\"ddspanid\":\"789019\",\"data\":{\"OrderIdentifier\":\"ORDER\", \"Items\":[{\"RecipeIdentifier\":\"1\"}]}}}";

    private IEventHandlerService eventHandlerService;
    
    @Mock
    private IRecipeRepository recipeRepository;
    
    @Resource
    private FunctionConfiguration configuration;
    
    @BeforeEach
    public void setUp() throws Exception {
        this.recipeRepository = Mockito.mock(IRecipeRepository.class);
        this.eventHandlerService = new EventHandlerService(this.recipeRepository);
        this.configuration = new FunctionConfiguration(this.eventHandlerService);
    }
    
    @Test
    public void GivenFunctionExectues_WhenRecipeIsFound()
    {
        Mockito.when(recipeRepository.findById(1L)).thenReturn(Optional.of(new Recipe()));
        
        SQSEvent.SQSMessage message = new SQSEvent.SQSMessage();
        message.setBody(data);
        message.setMessageId("Hello");
        
        ArrayList<SQSEvent.SQSMessage> messages = new ArrayList<>(1);
        messages.add(message);
        
        SQSEvent evt = new SQSEvent();
        evt.setRecords(messages);
        
        boolean response = this.configuration.processOrderConfirmedMessage(message);

        assertThat(response).isEqualTo(true);
    }

    @Test
    public void GivenFunctionExecutes_WhenRecipeIsNotFound_ShouldCompleteOk()
    {
        Mockito.when(recipeRepository.findById(1L)).thenReturn(Optional.empty());

        SQSEvent.SQSMessage message = new SQSEvent.SQSMessage();
        message.setBody(data);
        message.setMessageId("Hello");

        ArrayList<SQSEvent.SQSMessage> messages = new ArrayList<>(1);
        messages.add(message);

        SQSEvent evt = new SQSEvent();
        evt.setRecords(messages);

        boolean response = this.configuration.processOrderConfirmedMessage(message);
        
        assertThat(response).isEqualTo(true);
    }
}
