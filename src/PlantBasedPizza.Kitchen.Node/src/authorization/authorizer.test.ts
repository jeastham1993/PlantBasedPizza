import { Authorizer } from "./authorizer";
const jwt = require('jsonwebtoken');

const secretKey = "This is a very secret key, please don't use it anywhere else apart from here it is purely for test purposes'";

const authorizer = new Authorizer(secretKey);

describe("Authorizer", () => {
  test("ShouldAuthorizeSuccessfully", () => {

    var token = jwt.sign({ role: 'staff' }, secretKey);

    const result = authorizer.authorizeRequest(
      {
        headers: {
          Authorization:
            `Bearer ${token}`,
        },
        requestContext: {
          elb: {
            targetGroupArn: "",
          },
        },
        httpMethod: "POST",
        body: "",
        path: "",
        isBase64Encoded: false,
      },
      ["staff"],
    );

    expect(result).toBe(true);
  });

  test("ShouldFailToAuthorizeForInvalidRole", () => {
    var token = jwt.sign({ role: 'staff' }, secretKey);

    const result = authorizer.authorizeRequest(
      {
        headers: {
          Authorization:
            `Bearer ${token}`
        },
        requestContext: {
          elb: {
            targetGroupArn: "",
          },
        },
        httpMethod: "POST",
        body: "",
        path: "",
        isBase64Encoded: false,
      },
      ["admin"],
    );

    expect(result).toBe(false);
  });

  test("ShouldFailToAuthorizeNoHeaderProvided", () => {
    const result = authorizer.authorizeRequest(
      {
        headers: {},
        requestContext: {
          elb: {
            targetGroupArn: "",
          },
        },
        httpMethod: "POST",
        body: "",
        path: "",
        isBase64Encoded: false,
      },
      ["staff"],
    );

    expect(result).toBe(false);
  });
});
