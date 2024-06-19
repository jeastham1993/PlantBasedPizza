import com.recipe.api.RecipeApiApplication;
import org.junit.jupiter.api.Test;

import static org.springframework.boot.SpringApplication.run;

public class RecipeApiIT {
    @Test
    public void testApplicationStartup_shouldStartApplication()
    {
        run(RecipeApiApplication.class);
    }
}
