import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import {
  Box,
  Container,
  CssBaseline,
  Typography,
  Table,
  Snackbar,
  Button,
} from "@mui/joy";
import Grid from "@mui/joy/Grid";
import Sheet from "@mui/joy/Sheet";
import { ordersApi } from "../axiosConfig";
import Moment from "moment";
import { NotificationHub } from "./SignalR";

function OrderDetail(props) {
  const { orderNumber } = useParams();
  const [order, setOrder] = useState({ items: [], history: [] });
  const [snackbarOpen, setSnackbarOpen] = React.useState(false);
  const [snackbarContents, setSnackbarContents] = React.useState("");
  const hub = new NotificationHub((message) => {
    setSnackbarContents(message);
    setSnackbarOpen(true);
  });

  useEffect(() => {
    // Replace with your actual endpoint
    const fetchData = async () => {
      try {
        const response = await ordersApi.get(`/${orderNumber}/detail`);
        setOrder(response.data);
      } catch (error) {
        console.error("Error fetching the menu data:", error);
      }
    };

    fetchData();
  }, []);

  async function cancelOrder(order) {
    await ordersApi.post(`/${order.orderIdentifier}/cancel`, {
      orderIdentifier: order.orderIdentifier,
    });
    setSnackbarContents('Cancellation requested');
    setSnackbarOpen(true);
  }

  const handleSnackbarClose = () => {
    setSnackbarContents("");
    setSnackbarOpen(false);
  };

  return (
    <div>
      <Box sx={{ flexGrow: 1 }}>
        <CssBaseline />
        <Container sx={{ py: 1 }} maxWidth="xl">
          <Grid container spacing={1}>
            <Grid item>
              <Typography level="h2" gutterBottom>
                Order: {order.orderNumber}
              </Typography>
              <Button
                onClick={() => {
                  cancelOrder(order);
                }}
              >
                Cancel Order
              </Button>
              <Grid container spacing={2} sx={{ flexGrow: 1 }}>
                <Grid xs={6}>
                  <Typography level="h3" gutterBottom>
                    Items
                  </Typography>
                  <Sheet>
                    <Table>
                      <thead>
                        <tr>
                          <th style={{ width: "40%" }}>Item</th>
                          <th>Quantity</th>
                        </tr>
                      </thead>
                      <tbody>
                        {order.items.map((item) => (
                          <tr key={item.itemName}>
                            <td>{item.itemName}</td>
                            <td>{item.quantity}</td>
                          </tr>
                        ))}
                      </tbody>
                    </Table>
                  </Sheet>
                </Grid>
                <Grid item xs={6}>
                  <Typography level="h3" gutterBottom>
                    History
                  </Typography>
                  <Sheet>
                    <Table>
                      <thead>
                        <tr>
                          <th style={{ width: "40%" }}>Item</th>
                          <th>Quantity</th>
                        </tr>
                      </thead>
                      <tbody>
                        {order.history.map((item) => (
                          <tr
                            key={Moment(item.historyDate).format(
                              "DD/MM/YYYY HH:mm:ss"
                            )}
                          >
                            <td>
                              {Moment(item.historyDate).format(
                                "DD/MM/YYYY HH:mm:ss"
                              )}
                            </td>
                            <td>{item.description}</td>
                          </tr>
                        ))}
                      </tbody>
                    </Table>
                  </Sheet>
                </Grid>
              </Grid>
            </Grid>
          </Grid>
        </Container>
      </Box>
      <Box sx={{ width: 500 }}>
        <Snackbar
          autoHideDuration={2000}
          variant="solid"
          color="success"
          anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
          open={snackbarOpen}
          onClose={handleSnackbarClose}
        >
          {snackbarContents}
        </Snackbar>
      </Box>
    </div>
  );
}

export default OrderDetail;
