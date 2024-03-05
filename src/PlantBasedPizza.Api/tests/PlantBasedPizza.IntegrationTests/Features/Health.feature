Feature: HealthCheckFeature
All features related to the healthcheck module.

@health
Scenario: HealthCheckIsSuccess
	Given the application is running
	Then a 200 status code is returned
