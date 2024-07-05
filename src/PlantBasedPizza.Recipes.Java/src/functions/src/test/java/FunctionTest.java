import com.amazonaws.services.lambda.runtime.events.SQSEvent;
import com.recipe.functions.FunctionConfiguration;
import org.junit.jupiter.api.Test;

import java.util.ArrayList;

public class FunctionTest {
    private String data = "{\"detail\": {\"specversion\": \"1.0\",\"type\": \"com.example.someevent\",\"source\": \"/mycontext\",\"id\": \"A234-1234-1234\",\"time\": \"2018-04-05T17:31:00Z\",\"comexampleextension1\": \"value\",\"comexampleothervalue\": 5,\"datacontenttype\": \"application/json\",\"data\": {\"orderIdentifier\": \"ORDER\"}}}";
    
    @Test
    public void TestExecutionOfFunction()
    {
        FunctionConfiguration configuration = new FunctionConfiguration();

        SQSEvent.SQSMessage message = new SQSEvent.SQSMessage();
        message.setBody(data);
        message.setMessageId("Hello");
        
        ArrayList<SQSEvent.SQSMessage> messages = new ArrayList<>(1);
        messages.add(message);
        
        SQSEvent evt = new SQSEvent();
        evt.setRecords(messages);
        
        configuration.processOrderConfirmedMessage(message);
    }
}
