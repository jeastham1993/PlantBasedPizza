package com.recipe.api.configs;
import com.google.gson.Gson;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.jdbc.datasource.SimpleDriverDataSource;
import org.springframework.orm.jpa.JpaTransactionManager;
import org.springframework.orm.jpa.JpaVendorAdapter;
import org.springframework.orm.jpa.LocalContainerEntityManagerFactoryBean;
import org.springframework.orm.jpa.vendor.Database;
import org.springframework.orm.jpa.vendor.HibernateJpaVendorAdapter;
import jakarta.persistence.EntityManagerFactory;
import software.amazon.lambda.powertools.parameters.ParamManager;
import software.amazon.lambda.powertools.parameters.SSMProvider;

import javax.sql.DataSource;

@Configuration
public class JpaConfiguration {
    private final Gson gson = new Gson();
    public JpaConfiguration()
    {
    }

    public DataSource dataSource() {
        SSMProvider ssmProvider = ParamManager.getSsmProvider()
                .withDecryption();
        
        String dbConnectionString = System.getenv("DB_CONNECTION_STRING");
        
        if (dbConnectionString == null){
            dbConnectionString = ssmProvider.get(System.getenv("DB_PARAMETER_NAME")); 
        }

        var dataSource = new SimpleDriverDataSource();
        dataSource.setDriverClass(org.postgresql.Driver.class);
        dataSource.setUrl(dbConnectionString);

        return dataSource;
    }

    @Bean
    public JpaTransactionManager transactionManager(EntityManagerFactory emf) {
        return new JpaTransactionManager(emf);
    }

    @Bean
    public JpaVendorAdapter jpaVendorAdapter() {
        HibernateJpaVendorAdapter jpaVendorAdapter = new HibernateJpaVendorAdapter();
        jpaVendorAdapter.setDatabase(Database.POSTGRESQL);
        jpaVendorAdapter.setGenerateDdl(true);

        return jpaVendorAdapter;
    }

    @Bean
    public LocalContainerEntityManagerFactoryBean entityManagerFactory() {
        LocalContainerEntityManagerFactoryBean lemfb = new LocalContainerEntityManagerFactoryBean();
        lemfb.setDataSource(dataSource());
        lemfb.setJpaVendorAdapter(jpaVendorAdapter());
        lemfb.setPackagesToScan("com.recipe.api", "com.recipe.core");
        return lemfb;
    }
}