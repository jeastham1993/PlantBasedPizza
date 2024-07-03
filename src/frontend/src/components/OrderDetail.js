import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import {
  Box,
  Container,
  CssBaseline,
  Typography,
  Table,
} from "@mui/joy";
import Grid from '@mui/joy/Grid';
import Sheet from '@mui/joy/Sheet';
import { ordersApi } from "../axiosConfig";
import Moment from "moment";

function OrderDetail(props) {
  const { orderNumber } = useParams();
  const [order, setOrder] = useState({ items: [], history: [] });

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

  return (
    <Box sx={{ flexGrow: 1 }}>
      <CssBaseline />
      <Container sx={{ py: 1 }} maxWidth="xl">
        <Grid container spacing={1}>
          <Grid item>
            <Typography level="h2" gutterBottom>
              Order: {order.orderNumber}
            </Typography>
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
                          <tr>
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
                          <tr>
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
  );
}

export default OrderDetail;
