namespace ArchitectureAI.Application.Common;

public static class EmailTemplates
{
    public static string GetDashboardWelcomeEmail(string userName, string dashboardLink)
    {
        return $@"
<!doctype html>
<html>
  <head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <style>
      /* Reset & Basics */
      body {{
        font-family: ""Helvetica Neue"", Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        font-size: 14px;
        line-height: 1.5;
        margin: 0;
        padding: 0;
        -ms-text-size-adjust: 100%;
        -webkit-text-size-adjust: 100%;
        background-color: #f6f9fc;
        color: #1a1a1a;
      }}
      table {{
        border-collapse: separate;
        mso-table-lspace: 0pt;
        mso-table-rspace: 0pt;
        width: 100%;
      }}
      table td {{
        font-family: sans-serif;
        font-size: 14px;
        vertical-align: top;
      }}

      /* Layout */
      .body {{
        background-color: #f6f9fc;
        width: 100%;
      }}
      .container {{
        display: block;
        margin: 0 auto !important;
        max-width: 580px;
        padding: 10px;
        width: 580px;
      }}
      .content {{
        box-sizing: border-box;
        display: block;
        margin: 0 auto;
        max-width: 580px;
        padding: 10px;
      }}
      .main {{
        background: #ffffff;
        border-radius: 8px;
        width: 100%;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
      }}
      .wrapper {{
        box-sizing: border-box;
        padding: 30px;
      }}

      /* Branding */
      .branding {{
        text-align: center;
        padding-bottom: 20px;
      }}
      .branding h2 {{
        margin: 0;
        color: #00af8e;
        font-size: 24px;
        font-weight: bold;
        letter-spacing: -0.5px;
      }}

      /* Typography */
      h1,
      h2,
      h3,
      h4 {{
        color: #1a1a1a;
        font-family: sans-serif;
        font-weight: 600;
        line-height: 1.4;
        margin: 0;
        margin-bottom: 20px;
      }}
      p,
      ul,
      ol {{
        font-family: sans-serif;
        font-size: 16px;
        font-weight: normal;
        margin: 0;
        margin-bottom: 15px;
        color: #4a5568;
      }}

      /* Buttons */
      .btn {{
        box-sizing: border-box;
        width: 100%;
        margin-bottom: 25px;
      }}
      .btn table {{
        width: 100%;
      }}
      .btn table td {{
        padding-bottom: 15px;
      }}
      .btn table td a {{
        background-color: #00af8e;
        border: solid 1px #00af8e;
        border-radius: 6px;
        box-sizing: border-box;
        color: #ffffff;
        cursor: pointer;
        display: inline-block;
        font-size: 16px;
        font-weight: bold;
        margin: 0;
        padding: 12px 25px;
        text-decoration: none;
        text-transform: capitalize;
        box-shadow: 0 2px 4px rgba(0, 175, 142, 0.2);
      }}
      .btn-primary table td a {{
        background-color: #00af8e;
        border-color: #00af8e;
        color: #ffffff;
      }}

      /* Utilities */
      .align-center {{
        text-align: center;
      }}
      .text-sm {{
        font-size: 13px;
        color: #718096;
      }}
      .text-xs {{
        font-size: 12px;
        color: #a0aec0;
      }}
      .hr {{
        border-top: 1px solid #e2e8f0;
        margin: 20px 0;
      }}

      /* Footer */
      .footer {{
        clear: both;
        margin-top: 10px;
        text-align: center;
        width: 100%;
      }}
      .footer td,
      .footer p,
      .footer span,
      .footer a {{
        color: #999999;
        font-size: 12px;
        text-align: center;
      }}

      /* Responsive */
      @media only screen and (max-width: 620px) {{
        .main {{
          border-radius: 0;
        }}
        .container {{
          padding: 0 !important;
          width: 100% !important;
        }}
        .content {{
          padding: 0 !important;
        }}
        .wrapper {{
          padding: 20px !important;
        }}
      }}
    </style>
  </head>
  <body>
    <table
      role=""presentation""
      border=""0""
      cellpadding=""0""
      cellspacing=""0""
      class=""body""
    >
      <tr>
        <td>&nbsp;</td>
        <td class=""container"">
          <div class=""content"">
            <!-- START CENTERED WHITE CONTAINER -->
            <table role=""presentation"" class=""main"">
              <tr>
                <td class=""wrapper"">
                  <table
                    role=""presentation""
                    border=""0""
                    cellpadding=""0""
                    cellspacing=""0""
                  >
                    <tr>
                      <td>
                        <!-- Branding -->
                        <div class=""branding"">
                          <h2>ArchitectureAI</h2>
                        </div>

                        <!-- Greeting -->
                        <p>Hi {userName},</p>

                        <!-- Content -->
                        <p>
                          Welcome to ArchitectureAI! We're thrilled to have you
                          on board.
                        </p>
                        <p>
                          You now have access to our powerful tools for
                          analyzing and designing your cloud architecture. Get
                          started by visiting your dashboard.
                        </p>

                        <!-- Action Button -->
                        <table
                          role=""presentation""
                          border=""0""
                          cellpadding=""0""
                          cellspacing=""0""
                          class=""btn btn-primary""
                        >
                          <tbody>
                            <tr>
                              <td align=""center"">
                                <table
                                  role=""presentation""
                                  border=""0""
                                  cellpadding=""0""
                                  cellspacing=""0""
                                >
                                  <tbody>
                                    <tr>
                                      <td>
                                        <a
                                          href=""{dashboardLink}""
                                          target=""_blank""
                                          >Go to Dashboard</a
                                        >
                                      </td>
                                    </tr>
                                  </tbody>
                                </table>
                              </td>
                            </tr>
                          </tbody>
                        </table>

                        <p>
                          If you have any questions or need help, feel free to
                          reply to this email. We're here to support you!
                        </p>

                        <div class=""hr""></div>

                        <!-- Sign off -->
                        <p class=""text-sm"" style=""margin-top: 20px"">
                          Happy Architecting,<br />The ArchitectureAI Team
                        </p>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>

              <!-- END MAIN CONTENT AREA -->
            </table>

            <!-- START FOOTER -->
            <div class=""footer"">
              <table
                role=""presentation""
                border=""0""
                cellpadding=""0""
                cellspacing=""0""
              >
                <tr>
                  <td class=""content-block"">
                    <span class=""apple-link""
                      >ArchitectureAI Inc, 123 Cloud Way, Tech City</span
                    >
                    <br />
                    Don't like these emails? <a href=""#"">Unsubscribe</a>.
                  </td>
                </tr>
              </table>
            </div>
            <!-- END FOOTER -->
          </div>
        </td>
        <td>&nbsp;</td>
      </tr>
    </table>
  </body>
</html>";
    }

    public static string GetVerifyEmail(string userName, string verificationLink)
    {
        return $@"
<!doctype html>
<html>
  <head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <style>
      /* Reset & Basics */
      body {{
        font-family: ""Helvetica Neue"", Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        font-size: 14px;
        line-height: 1.5;
        margin: 0;
        padding: 0;
        -ms-text-size-adjust: 100%;
        -webkit-text-size-adjust: 100%;
        background-color: #f6f9fc;
        color: #1a1a1a;
      }}
      table {{
        border-collapse: separate;
        mso-table-lspace: 0pt;
        mso-table-rspace: 0pt;
        width: 100%;
      }}
      table td {{
        font-family: sans-serif;
        font-size: 14px;
        vertical-align: top;
      }}

      /* Layout */
      .body {{
        background-color: #f6f9fc;
        width: 100%;
      }}
      .container {{
        display: block;
        margin: 0 auto !important;
        max-width: 580px;
        padding: 10px;
        width: 580px;
      }}
      .content {{
        box-sizing: border-box;
        display: block;
        margin: 0 auto;
        max-width: 580px;
        padding: 10px;
      }}
      .main {{
        background: #ffffff;
        border-radius: 8px;
        width: 100%;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
      }}
      .wrapper {{
        box-sizing: border-box;
        padding: 30px;
      }}

      /* Branding */
      .branding {{
        text-align: center;
        padding-bottom: 20px;
      }}
      .branding h2 {{
        margin: 0;
        color: #00af8e;
        font-size: 24px;
        font-weight: bold;
        letter-spacing: -0.5px;
      }}

      /* Typography */
      h1,
      h2,
      h3,
      h4 {{
        color: #1a1a1a;
        font-family: sans-serif;
        font-weight: 600;
        line-height: 1.4;
        margin: 0;
        margin-bottom: 20px;
      }}
      p,
      ul,
      ol {{
        font-family: sans-serif;
        font-size: 16px;
        font-weight: normal;
        margin: 0;
        margin-bottom: 15px;
        color: #4a5568;
      }}

      /* Buttons */
      .btn {{
        box-sizing: border-box;
        width: 100%;
        margin-bottom: 25px;
      }}
      .btn table {{
        width: 100%;
      }}
      .btn table td {{
        padding-bottom: 15px;
      }}
      .btn table td a {{
        background-color: #00af8e;
        border: solid 1px #00af8e;
        border-radius: 6px;
        box-sizing: border-box;
        color: #ffffff;
        cursor: pointer;
        display: inline-block;
        font-size: 16px;
        font-weight: bold;
        margin: 0;
        padding: 12px 25px;
        text-decoration: none;
        text-transform: capitalize;
        box-shadow: 0 2px 4px rgba(0, 175, 142, 0.2);
      }}
      .btn-primary table td a {{
        background-color: #00af8e;
        border-color: #00af8e;
        color: #ffffff;
      }}

      /* Utilities */
      .align-center {{
        text-align: center;
      }}
      .text-sm {{
        font-size: 13px;
        color: #718096;
      }}
      .text-xs {{
        font-size: 12px;
        color: #a0aec0;
      }}
      .link-primary {{
        color: #00af8e;
        text-decoration: underline;
      }}
      .hr {{
        border-top: 1px solid #e2e8f0;
        margin: 20px 0;
      }}

      /* Footer */
      .footer {{
        clear: both;
        margin-top: 10px;
        text-align: center;
        width: 100%;
      }}
      .footer td,
      .footer p,
      .footer span,
      .footer a {{
        color: #999999;
        font-size: 12px;
        text-align: center;
      }}

      /* Responsive */
      @media only screen and (max-width: 620px) {{
        .main {{
          border-radius: 0;
        }}
        .container {{
          padding: 0 !important;
          width: 100% !important;
        }}
        .content {{
          padding: 0 !important;
        }}
        .wrapper {{
          padding: 20px !important;
        }}
      }}
    </style>
  </head>
  <body>
    <table
      role=""presentation""
      border=""0""
      cellpadding=""0""
      cellspacing=""0""
      class=""body""
    >
      <tr>
        <td>&nbsp;</td>
        <td class=""container"">
          <div class=""content"">
            <!-- START CENTERED WHITE CONTAINER -->
            <table role=""presentation"" class=""main"">
              <tr>
                <td class=""wrapper"">
                  <table
                    role=""presentation""
                    border=""0""
                    cellpadding=""0""
                    cellspacing=""0""
                  >
                    <tr>
                      <td>
                        <!-- Branding -->
                        <div class=""branding"">
                          <h2>ArchitectureAI</h2>
                        </div>

                        <!-- Greeting -->
                        <p>Hi {userName},</p>

                        <!-- Content -->
                        <p>
                          Thanks for getting started with ArchitectureAI! We
                          need to verify your email address to complete your
                          account setup and ensure your account is secure.
                        </p>

                        <!-- Action Button -->
                        <table
                          role=""presentation""
                          border=""0""
                          cellpadding=""0""
                          cellspacing=""0""
                          class=""btn btn-primary""
                        >
                          <tbody>
                            <tr>
                              <td align=""center"">
                                <table
                                  role=""presentation""
                                  border=""0""
                                  cellpadding=""0""
                                  cellspacing=""0""
                                >
                                  <tbody>
                                    <tr>
                                      <td>
                                        <a
                                          href=""{verificationLink}""
                                          target=""_blank""
                                          >Verify Email Address</a
                                        >
                                      </td>
                                    </tr>
                                  </tbody>
                                </table>
                              </td>
                            </tr>
                          </tbody>
                        </table>

                        <!-- Fallback Link -->
                        <p class=""text-sm"">
                          Or copy and paste this link into your browser:
                        </p>
                        <p class=""text-sm"">
                          <a href=""{verificationLink}"" class=""link-primary""
                            >{verificationLink}</a
                          >
                        </p>

                        <div class=""hr""></div>

                        <!-- Expiry Notice -->
                        <p class=""text-xs"">
                          This link will expire in 24 hours. If you didn't
                          create an account with ArchitectureAI, you can safely
                          ignore this email.
                        </p>

                        <!-- Sign off -->
                        <p class=""text-sm"" style=""margin-top: 20px"">
                          Best,<br />The ArchitectureAI Team
                        </p>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>

              <!-- END MAIN CONTENT AREA -->
            </table>

            <!-- START FOOTER -->
            <div class=""footer"">
              <table
                role=""presentation""
                border=""0""
                cellpadding=""0""
                cellspacing=""0""
              >
                <tr>
                  <td class=""content-block"">
                    <span class=""apple-link""
                      >ArchitectureAI Inc, 123 Cloud Way, Tech City</span
                    >
                    <br />
                    Don't like these emails? <a href=""#"">Unsubscribe</a>.
                  </td>
                </tr>
              </table>
            </div>
            <!-- END FOOTER -->
          </div>
        </td>
        <td>&nbsp;</td>
      </tr>
    </table>
  </body>
</html>";
    }

    public static string GetPasswordResetEmail(string userName, string resetLink)
    {
        return $@"
<!doctype html>
<html>
  <head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <style>
      /* Reset & Basics */
      body {{
        font-family: ""Helvetica Neue"", Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        font-size: 14px;
        line-height: 1.5;
        margin: 0;
        padding: 0;
        -ms-text-size-adjust: 100%;
        -webkit-text-size-adjust: 100%;
        background-color: #f6f9fc;
        color: #1a1a1a;
      }}
      table {{
        border-collapse: separate;
        mso-table-lspace: 0pt;
        mso-table-rspace: 0pt;
        width: 100%;
      }}
      table td {{
        font-family: sans-serif;
        font-size: 14px;
        vertical-align: top;
      }}

      /* Layout */
      .body {{
        background-color: #f6f9fc;
        width: 100%;
      }}
      .container {{
        display: block;
        margin: 0 auto !important;
        max-width: 580px;
        padding: 10px;
        width: 580px;
      }}
      .content {{
        box-sizing: border-box;
        display: block;
        margin: 0 auto;
        max-width: 580px;
        padding: 10px;
      }}
      .main {{
        background: #ffffff;
        border-radius: 8px;
        width: 100%;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
      }}
      .wrapper {{
        box-sizing: border-box;
        padding: 30px;
      }}

      /* Branding */
      .branding {{
        text-align: center;
        padding-bottom: 20px;
      }}
      .branding h2 {{
        margin: 0;
        color: #00af8e;
        font-size: 24px;
        font-weight: bold;
        letter-spacing: -0.5px;
      }}

      /* Typography */
      h1,
      h2,
      h3,
      h4 {{
        color: #1a1a1a;
        font-family: sans-serif;
        font-weight: 600;
        line-height: 1.4;
        margin: 0;
        margin-bottom: 20px;
      }}
      p,
      ul,
      ol {{
        font-family: sans-serif;
        font-size: 16px;
        font-weight: normal;
        margin: 0;
        margin-bottom: 15px;
        color: #4a5568;
      }}

      /* Buttons */
      .btn {{
        box-sizing: border-box;
        width: 100%;
        margin-bottom: 25px;
      }}
      .btn table {{
        width: 100%;
      }}
      .btn table td {{
        padding-bottom: 15px;
      }}
      .btn table td a {{
        background-color: #00af8e;
        border: solid 1px #00af8e;
        border-radius: 6px;
        box-sizing: border-box;
        color: #ffffff;
        cursor: pointer;
        display: inline-block;
        font-size: 16px;
        font-weight: bold;
        margin: 0;
        padding: 12px 25px;
        text-decoration: none;
        text-transform: capitalize;
        box-shadow: 0 2px 4px rgba(0, 175, 142, 0.2);
      }}
      .btn-primary table td a {{
        background-color: #00af8e;
        border-color: #00af8e;
        color: #ffffff;
      }}

      /* Utilities */
      .align-center {{
        text-align: center;
      }}
      .text-sm {{
        font-size: 13px;
        color: #718096;
      }}
      .text-xs {{
        font-size: 12px;
        color: #a0aec0;
      }}
      .link-primary {{
        color: #00af8e;
        text-decoration: underline;
      }}
      .hr {{
        border-top: 1px solid #e2e8f0;
        margin: 20px 0;
      }}

      /* Footer */
      .footer {{
        clear: both;
        margin-top: 10px;
        text-align: center;
        width: 100%;
      }}
      .footer td,
      .footer p,
      .footer span,
      .footer a {{
        color: #999999;
        font-size: 12px;
        text-align: center;
      }}

      /* Responsive */
      @media only screen and (max-width: 620px) {{
        .main {{
          border-radius: 0;
        }}
        .container {{
          padding: 0 !important;
          width: 100% !important;
        }}
        .content {{
          padding: 0 !important;
        }}
        .wrapper {{
          padding: 20px !important;
        }}
      }}
    </style>
  </head>
  <body>
    <table
      role=""presentation""
      border=""0""
      cellpadding=""0""
      cellspacing=""0""
      class=""body""
    >
      <tr>
        <td>&nbsp;</td>
        <td class=""container"">
          <div class=""content"">
            <!-- START CENTERED WHITE CONTAINER -->
            <table role=""presentation"" class=""main"">
              <tr>
                <td class=""wrapper"">
                  <table
                    role=""presentation""
                    border=""0""
                    cellpadding=""0""
                    cellspacing=""0""
                  >
                    <tr>
                      <td>
                        <!-- Branding -->
                        <div class=""branding"">
                          <h2>ArchitectureAI</h2>
                        </div>

                        <!-- Greeting -->
                        <p>Hi {userName},</p>

                        <!-- Content -->
                        <p>
                          We received a request to reset the password for your
                          ArchitectureAI account. If you didn't make this
                          request, you can safely ignore this email.
                        </p>

                        <!-- Action Button -->
                        <table
                          role=""presentation""
                          border=""0""
                          cellpadding=""0""
                          cellspacing=""0""
                          class=""btn btn-primary""
                        >
                          <tbody>
                            <tr>
                              <td align=""center"">
                                <table
                                  role=""presentation""
                                  border=""0""
                                  cellpadding=""0""
                                  cellspacing=""0""
                                >
                                  <tbody>
                                    <tr>
                                      <td>
                                        <a href=""{resetLink}"" target=""_blank""
                                          >Reset Password</a
                                        >
                                      </td>
                                    </tr>
                                  </tbody>
                                </table>
                              </td>
                            </tr>
                          </tbody>
                        </table>

                        <!-- Fallback Link -->
                        <p class=""text-sm"">
                          Or copy and paste this link into your browser:
                        </p>
                        <p class=""text-sm"">
                          <a href=""{resetLink}"" class=""link-primary""
                            >{resetLink}</a
                          >
                        </p>

                        <div class=""hr""></div>

                        <!-- Expiry Notice -->
                        <p class=""text-xs"">
                          This link will expire in 1 hour. If you didn't request
                          a password reset, please ignore this email or contact
                          support if you have questions.
                        </p>

                        <!-- Sign off -->
                        <p class=""text-sm"" style=""margin-top: 20px"">
                          Best,<br />The ArchitectureAI Team
                        </p>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>

              <!-- END MAIN CONTENT AREA -->
            </table>

            <!-- START FOOTER -->
            <div class=""footer"">
              <table
                role=""presentation""
                border=""0""
                cellpadding=""0""
                cellspacing=""0""
              >
                <tr>
                  <td class=""content-block"">
                    <span class=""apple-link""
                      >ArchitectureAI Inc, 123 Cloud Way, Tech City</span
                    >
                    <br />
                    Don't like these emails? <a href=""#"">Unsubscribe</a>.
                  </td>
                </tr>
              </table>
            </div>
            <!-- END FOOTER -->
          </div>
        </td>
        <td>&nbsp;</td>
      </tr>
    </table>
  </body>
</html>";
    }

    public static string GetPasswordChangedEmail(string userName)
    {
        return $@"
<!doctype html>
<html>
  <head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <style>
      /* Reset & Basics */
      body {{
        font-family: ""Helvetica Neue"", Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        font-size: 14px;
        line-height: 1.5;
        margin: 0;
        padding: 0;
        -ms-text-size-adjust: 100%;
        -webkit-text-size-adjust: 100%;
        background-color: #f6f9fc;
        color: #1a1a1a;
      }}
      table {{
        border-collapse: separate;
        mso-table-lspace: 0pt;
        mso-table-rspace: 0pt;
        width: 100%;
      }}
      table td {{
        font-family: sans-serif;
        font-size: 14px;
        vertical-align: top;
      }}

      /* Layout */
      .body {{
        background-color: #f6f9fc;
        width: 100%;
      }}
      .container {{
        display: block;
        margin: 0 auto !important;
        max-width: 580px;
        padding: 10px;
        width: 580px;
      }}
      .content {{
        box-sizing: border-box;
        display: block;
        margin: 0 auto;
        max-width: 580px;
        padding: 10px;
      }}
      .main {{
        background: #ffffff;
        border-radius: 8px;
        width: 100%;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
      }}
      .wrapper {{
        box-sizing: border-box;
        padding: 30px;
      }}

      /* Branding */
      .branding {{
        text-align: center;
        padding-bottom: 20px;
      }}
      .branding h2 {{
        margin: 0;
        color: #00af8e;
        font-size: 24px;
        font-weight: bold;
        letter-spacing: -0.5px;
      }}

      /* Utilities */
      .alert-warning {{
        background-color: #fffaf0;
        border-left: 4px solid #ed8936;
        color: #2d3748;
        padding: 15px;
        margin-bottom: 20px;
        border-radius: 4px;
      }}
      .text-sm {{
        font-size: 13px;
        color: #718096;
      }}

      /* Typography */
      h1,
      h2,
      h3,
      h4 {{
        color: #1a1a1a;
        font-family: sans-serif;
        font-weight: 600;
        line-height: 1.4;
        margin: 0;
        margin-bottom: 20px;
      }}
      p,
      ul,
      ol {{
        font-family: sans-serif;
        font-size: 16px;
        font-weight: normal;
        margin: 0;
        margin-bottom: 15px;
        color: #4a5568;
      }}

      /* Footer */
      .footer {{
        clear: both;
        margin-top: 10px;
        text-align: center;
        width: 100%;
      }}
      .footer td,
      .footer p,
      .footer span,
      .footer a {{
        color: #999999;
        font-size: 12px;
        text-align: center;
      }}

      /* Responsive */
      @media only screen and (max-width: 620px) {{
        .main {{
          border-radius: 0;
        }}
        .container {{
          padding: 0 !important;
          width: 100% !important;
        }}
        .content {{
          padding: 0 !important;
        }}
        .wrapper {{
          padding: 20px !important;
        }}
      }}
    </style>
  </head>
  <body>
    <table
      role=""presentation""
      border=""0""
      cellpadding=""0""
      cellspacing=""0""
      class=""body""
    >
      <tr>
        <td>&nbsp;</td>
        <td class=""container"">
          <div class=""content"">
            <!-- START CENTERED WHITE CONTAINER -->
            <table role=""presentation"" class=""main"">
              <tr>
                <td class=""wrapper"">
                  <table
                    role=""presentation""
                    border=""0""
                    cellpadding=""0""
                    cellspacing=""0""
                  >
                    <tr>
                      <td>
                        <!-- Branding -->
                        <div class=""branding"">
                          <h2>ArchitectureAI</h2>
                        </div>

                        <!-- Greeting -->
                        <p>Hi {userName},</p>

                        <!-- Content -->
                        <p>
                          This email is to confirm that the password for your
                          ArchitectureAI account was recently changed.
                        </p>

                        <!-- Security Alert -->
                        <div class=""alert-warning"">
                          <strong>Security Alert:</strong> If this wasn't you,
                          please contact our support team immediately to secure
                          your account.
                        </div>

                        <p>
                          If you did change your password, you can safely ignore
                          this email.
                        </p>

                        <!-- Sign off -->
                        <p class=""text-sm"" style=""margin-top: 20px"">
                          Best,<br />The ArchitectureAI Team
                        </p>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>

              <!-- END MAIN CONTENT AREA -->
            </table>

            <!-- START FOOTER -->
            <div class=""footer"">
              <table
                role=""presentation""
                border=""0""
                cellpadding=""0""
                cellspacing=""0""
              >
                <tr>
                  <td class=""content-block"">
                    <span class=""apple-link""
                      >ArchitectureAI Inc, 123 Cloud Way, Tech City</span
                    >
                    <br />
                    Don't like these emails? <a href=""#"">Unsubscribe</a>.
                  </td>
                </tr>
              </table>
            </div>
            <!-- END FOOTER -->
          </div>
        </td>
        <td>&nbsp;</td>
      </tr>
    </table>
  </body>
</html>";
    }
}
