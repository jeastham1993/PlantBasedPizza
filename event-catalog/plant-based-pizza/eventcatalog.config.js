/** @type {import('@eventcatalog/core/bin/eventcatalog.config').Config} */
export default {
  title: "EventCatalog",
  tagline: "Discover, Explore and Document your Event Driven Architectures",
  organizationName: "PlantBasedPizza",
  homepageLink: "https://eventcatalog.dev/",
  editUrl: "https://github.com/boyney123/eventcatalog-demo/edit/master",
  // By default set to false, add true to get urls ending in /
  trailingSlash: false,
  // Change to make the base url of the site different, by default https://{website}.com/docs,
  // changing to /company would be https://{website}.com/company/docs,
  base: "/",
  // Customize the logo, add your logo to public/ folder
  logo: {
    alt: "EventCatalog Logo",
    src: "/logo.png",
    text: "EventCatalog",
  },
  // required random generated id used by eventcatalog
  cId: "1862cd06-c150-45c4-aa0c-0e59b809adb9",
  generators: [
    [
      "@eventcatalog/generator-asyncapi",
      {
        services: [
          { path: "./asyncapi-files/orders-service.yml", id: "Orders Service" },
        ],
        domain: { id: "orders", name: "Orders", version: "0.0.1" },

        // Run in debug mode, for extra output, if your AsyncAPI fails to parse, it will tell you why
        debug: true,
      },
    ],
    [
      "@eventcatalog/generator-asyncapi",
      {
        services: [
          { path: "./asyncapi-files/payment-service.yml", id: "Payment Service" },
        ],
        domain: { id: "payment", name: "Payment", version: "0.0.1" },

        // Run in debug mode, for extra output, if your AsyncAPI fails to parse, it will tell you why
        debug: true,
      },
    ],
    [
      "@eventcatalog/generator-asyncapi",
      {
        services: [
          { path: "./asyncapi-files/delivery-service.yml", id: "Delivery Service" },
        ],
        domain: { id: "delivery", name: "Delivery", version: "0.0.1" },

        // Run in debug mode, for extra output, if your AsyncAPI fails to parse, it will tell you why
        debug: true,
      },
    ],
    [
      "@eventcatalog/generator-asyncapi",
      {
        services: [
          { path: "./asyncapi-files/kitchen-service.yml", id: "Kitchen Service" },
        ],
        domain: { id: "kitchen", name: "Kitchen", version: "0.0.1" },

        // Run in debug mode, for extra output, if your AsyncAPI fails to parse, it will tell you why
        debug: true,
      },
    ],
  ],
};
