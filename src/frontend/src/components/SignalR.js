import * as signalR from "@microsoft/signalr";

class NotificationHub {
  constructor(alert) {
    this.connection = null;
    this.alert = alert;
    this.initializeConnection();
  }

  initializeConnection() {
    // Only attempt to connect if a token exists
    const token = localStorage.getItem("token");
    if (!token || token === undefined) return;

    // Create the connection
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(
        "http://localhost:5090/notifications/orders",
        {
          transport: signalR.HttpTransportType.WebSockets,
          skipNegotiation: true,
          accessTokenFactory: () => token,
        }
      )
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Set up event listeners
    this.connection.on("paymentSuccess", (message) => {
      console.log("Payment complete:", message);
      if (typeof this.alert === "function") {
        this.alert("Payment successful");
      }
    });

    // Set up event listeners
    this.connection.on("preparing", (message) => {
      console.log("Order preparing:", message);
      if (typeof this.alert === "function") {
        this.alert("The kitchen is preparing your order");
      }
    });

    // Set up event listeners
    this.connection.on("prepComplete", (message) => {
      console.log("Order prep complete:", message);
      if (typeof this.alert === "function") {
        this.alert("It's prepared, your order is in the oven!");
      }
    });

    // Set up event listeners
    this.connection.on("readyForCollection", (message) => {
      console.log("Order ready for collection:", message);
      if (typeof this.alert === "function") {
        this.alert("Your order is ready for collection!");
      }
    });

    // Set up event listeners
    this.connection.on("bakeComplete", (message) => {
      console.log("Order baked:", message);
      if (typeof this.alert === "function") {
        this.alert("Your order is out of the oven, we just need to check the quality");
      }
    });

    // Set up event listeners
    this.connection.on("qualityCheckComplete", (message) => {
      console.log("Order quality checked:", message);
      if (typeof this.alert === "function") {
        this.alert("Wahooo, the quality meets the bar");
      }
    });

    // Set up event listeners
    this.connection.on("orderCancelled", (message) => {
      console.log("Order cancelled:", message);
      if (typeof this.alert === "function") {
        this.alert("Your order has been successfully cancelled");
      }
    });

    // Set up event listeners
    this.connection.on("cancellationFailed", (message) => {
      console.log("Order quality checked:", message);
      if (typeof this.alert === "function") {
        this.alert("Unfortunately, the kitchen has already started your order and it cannot be cancelled");
      }
    });

    // Start the connection
    this.connection
      .start()
      .then(() => {
        console.log("SignalR Connected");
      })
      .catch((error) => {
        console.error("SignalR Connection Error:", error);
      });
  }

  // Method to send messages if needed
  async sendMessage(methodName, ...args) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke(methodName, ...args);
      } catch (error) {
        console.error(`Error invoking ${methodName}:`, error);
      }
    }
  }

  // Cleanup method
  disconnect() {
    if (this.connection) {
      this.connection
        .stop()
        .then(() => console.log("SignalR Disconnected"))
        .catch((error) => console.error("SignalR Disconnect Error:", error));
    }
  }
}

// If using as a module
export { NotificationHub };
