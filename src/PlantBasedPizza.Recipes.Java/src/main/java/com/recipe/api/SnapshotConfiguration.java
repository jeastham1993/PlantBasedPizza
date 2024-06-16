package com.recipe.api;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.crac.Context;
import org.crac.Core;
import org.crac.Resource;
import org.springframework.context.annotation.Configuration;
import org.springframework.orm.jpa.LocalContainerEntityManagerFactoryBean;

import javax.sql.DataSource;
import java.sql.Connection;

@Configuration
public class SnapshotConfiguration implements Resource {
    private static final Logger LOG = LogManager.getLogger();
    LocalContainerEntityManagerFactoryBean _dataSourceBean;

    public SnapshotConfiguration(LocalContainerEntityManagerFactoryBean dataSourceBean)
    {
        LOG.info("Snapshot config constructor");

        Core.getGlobalContext().register(SnapshotConfiguration.this);

        _dataSourceBean = dataSourceBean;
    }

    @Override
    public void beforeCheckpoint(Context<? extends Resource> context) throws Exception {
        LOG.info("Before checkpoint");
        DataSource dataSource = _dataSourceBean.getDataSource();
        Connection databaseConnection = dataSource.getConnection();

        if (!databaseConnection.isClosed())
        {
            LOG.info("Closing connection");
            databaseConnection.close();
        }
    }

    @Override
    public void afterRestore(Context<? extends Resource> context) throws Exception {
        LOG.info("Restoring");
    }
}