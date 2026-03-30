# Code Review: VBLoginMockup — S219006685_P2019A-01

**Reviewed file:** `S219006685_P2019A-01/Form1.vb`
**Supporting file:** `S219006685_P2019A-01/Form1.Designer.vb`
**Framework:** VB.NET / .NET Framework 4.6.1 / Windows Forms

---

## Findings (Critical → Minor)

---

### [CRITICAL] 1. Logic error — response assigned before it is set

**Location:** `Form1.vb`, lines 27 and 33–40

```vb
txtResponse.Text = response   ' ← assigned HERE (response is still Nothing/"")
...
If Len(username.ToString) = 6 And password = correctPassword Then
    response = "Access Granted"
Else
    response = "program will be locked down"
End If
```

`txtResponse.Text` is assigned the value of `response` **before** the `If` block that actually sets `response`. On the first click the textbox will always be blank; on every subsequent click it will display the result from the **previous** click, not the current one.

**Fix:** Move `txtResponse.Text = response` to after the `End If`, or assign directly inside each branch:

```vb
If Len(username.ToString) = 6 And password = correctPassword Then
    txtResponse.Text = "Access Granted"
Else
    txtResponse.Text = "program will be locked down"
End If
```

---

### [CRITICAL] 2. Runtime exception on non-numeric input

**Location:** `Form1.vb`, lines 25–26

```vb
username = CDbl(txtUsername.Text)
password = CDbl(txtPassword.Text)
```

`CDbl()` throws an `InvalidCastException` / `FormatException` at runtime if the user types anything that is not a valid number (e.g. "admin", "abc123", an empty field). There is no `Try/Catch` block and no pre-validation. The application will crash with an unhandled exception dialog.

**Fix:** Use `Double.TryParse` and surface a friendly error message if parsing fails:

```vb
If Not Double.TryParse(txtUsername.Text, username) OrElse
   Not Double.TryParse(txtPassword.Text, password) Then
    txtResponse.Text = "Please enter numeric values."
    Exit Sub
End If
```

---

### [CRITICAL] 3. Wrong data type for credentials — `Double` instead of `String`

**Location:** `Form1.vb`, lines 16–17

```vb
Private username As Double
Private password As Double
```

Usernames and passwords are text, not numbers. Forcing them to `Double` introduces several problems:

- Leading zeros are silently dropped (`"006685"` → `6685.0`), so `Len(username.ToString)` returns `4`, never `6`, for any value that starts with a zero.
- Floating-point representation can silently alter values (e.g. very large integers lose precision).
- The semantic meaning of the variables is wrong, making the code misleading and hard to maintain.

**Fix:** Declare as `String` and handle comparison as strings. Remove the arithmetic "correct password" scheme entirely (see finding #4).

---

### [CRITICAL] 4. Security — authentication logic is a math trick, not real security

**Location:** `Form1.vb`, lines 30 and 33

```vb
correctPassword = username / 2
If ... And password = correctPassword Then
```

The "correct" password is simply `username ÷ 2`. Anyone who knows (or guesses) one value immediately knows the other. This is not authentication — it is an algebraic relationship. Additionally:

- Credentials are not hashed or encrypted.
- There is no account lockout after repeated failures.
- The locked-down message is cosmetic only; the application keeps running.

**Fix (for a real application):** Store credentials securely (hashed with bcrypt/Argon2 or verified against a backend). Never derive the password from the username mathematically. For this academic context, at minimum acknowledge these are placeholder validation rules.

---

### [HIGH] 5. `Len()` on a `Double` — unreliable string-length check

**Location:** `Form1.vb`, line 33

```vb
If Len(username.ToString) = 6 ...
```

`username` is `Double`. `username.ToString` will produce a string like `"219006"`, `"219006.0"`, or even scientific notation (`"2.19006E+05"`) depending on the value and locale settings. The check is not reliable:

- Any username starting with `0` is already broken (see finding #3).
- In cultures where the decimal separator is a comma, `ToString` may format differently.
- The intent (validate that the username is a 6-digit number) is obscured behind these conversions.

**Fix:** If the username must be validated as exactly 6 characters, check the raw input string before conversion: `If txtUsername.Text.Length = 6`.

---

### [HIGH] 6. Misleading variable names

**Location:** `Form1.vb`, lines 16–19

```vb
Private username As Double
Private password As Double
Private response As String
Private correctPassword As Double
```

`username` and `password` hold numeric values and are validated via an arithmetic relationship, not credential lookup. Calling them `username`/`password` misleads any reader into thinking real credential checking is happening. `response` shadows the VB.NET `My.Response` namespace name and is generic to the point of being uninformative.

**Fix:** Use names that reflect what the values actually are, e.g. `accessCode`, `enteredPin`, `expectedPin`, `loginResult`.

---

### [MEDIUM] 7. Module-level state is unnecessary

**Location:** `Form1.vb`, lines 16–19

All four variables (`username`, `password`, `response`, `correctPassword`) are declared at class level but are only ever used inside `btnLogin_Click`. Keeping them as fields means they persist stale values between button clicks, which directly contributes to the logic error in finding #1.

**Fix:** Declare them as local variables inside `btnLogin_Click`.

---

### [MEDIUM] 8. No reset/clear functionality

**Location:** `Form1.Designer.vb` (UI layout)

After a failed login attempt there is no way for the user to clear the fields without manually selecting and deleting text. A "Clear" or "Reset" button would improve usability.

**Fix:** Add a `btnClear` button that sets all three textboxes back to empty strings.

---

### [MINOR] 9. Unhelpful locked-down message and no user instructions

**Location:** `Form1.vb`, line 39; `Form1.Designer.vb`

- The failure message `"program will be locked down"` is alarming but inaccurate — the program does not actually lock down.
- The form has no labels explaining what format the username or password should be in.

**Fix:** Use a truthful message such as `"Invalid credentials. Please try again."` Add instructional labels or placeholder text to the input fields.

---

### [MINOR] 10. Typo in comment

**Location:** `Form1.vb`, line 29

```vb
'setting up the correct passsword to be half the username
```

`passsword` has three `s` characters.

---

## Summary Table

| # | Severity | Issue |
|---|----------|-------|
| 1 | Critical | `txtResponse.Text` assigned before `response` is set — always shows stale value |
| 2 | Critical | No error handling for non-numeric input — runtime crash on `CDbl()` |
| 3 | Critical | Credentials stored as `Double` — semantically wrong, drops leading zeros |
| 4 | Critical | Password = username / 2 — no real authentication, no hashing |
| 5 | High | `Len(username.ToString)` on a `Double` — locale-sensitive, unreliable |
| 6 | High | Misleading variable names (`username`/`password` hold numeric codes) |
| 7 | Medium | Class-level variables should be local to the button handler |
| 8 | Medium | No clear/reset button |
| 9 | Minor | Inaccurate locked-down message; no input format instructions |
| 10 | Minor | Typo: `passsword` in comment |
